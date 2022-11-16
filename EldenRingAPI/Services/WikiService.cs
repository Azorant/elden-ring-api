using EldenRingAPI.Interfaces;
using EldenRingAPI.Models;
using HtmlAgilityPack;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EldenRingAPI.Services
{
    public class WikiService : IWikiService
    {
        private readonly ILogger logger;
        private readonly Database db;

        public WikiService(ILogger<WikiService> logger, Database db)
        {
            this.logger = logger;
            this.db = db;
        }

        public async Task<string> fetchPage(string url)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetStringAsync(url);
            return response;
        }

        private string clean(string text, bool parse = true)
        {
            string cleaned = Regex.Replace(text, @"/(\n)|(&nbsp;)", "").Trim();
            return (cleaned == "-" || cleaned == "--") && parse ? "0" : cleaned;
        }

        private string titleCase(string text)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            return textInfo.ToTitleCase(text);
        }

        private WeaponAffinity parseWeaponTable(HtmlNode node, bool isSeal)
        {
            WeaponAffinity affinity = new();

            var nodes = node.SelectNodes($"{node.XPath}/tr[position()>2]");
            for (var n = 0; n < nodes.Count; n++)
            {
                var row = nodes[n];
                WeaponLevel level = new();

                if (n == 0)
                {
                    affinity.name = row.SelectSingleNode($"{row.XPath}/th").InnerText;
                }
                else
                {
                    level.level = int.Parse(row.SelectSingleNode($"{row.XPath}/th").InnerText.Split("+")[1]);
                }

                var columns = row.SelectNodes($"{row.XPath}/td");

                var crit = node.SelectSingleNode("//*[@id=\"infobox\"]/div/table/tbody/tr[3]/td[1]/div/span[5]");
                if (crit != null)
                {
                    if (Regex.Match(clean(crit.InnerText), @"\w+\s*\d+").Success)
                    {
                        level.attack.critical = int.Parse(Regex.Replace(clean(crit.InnerText), @"\D*", ""));
                    }
                    else level.attack.critical = int.Parse(clean(crit.NextSibling.InnerText));
                }
                else level.attack.critical = 0;

                var range = node.SelectSingleNode("//*[@id=\"infobox\"]/div/table/tbody/tr[3]/td[1]/div/span[6]");
                if (range != null && !isSeal)
                {
                    level.attack.range = int.Parse(clean(range.NextSibling.InnerText));
                }
                else level.attack.range = 0;

                #region Attribute columns
                for (var c = 0; c < (isSeal ? columns.Count : columns.Count + 1); c++)
                {
                    int value = int.TryParse(columns[c].InnerText, out value) ? value : 0;
                    string? scaling = columns[c].InnerText == "-" ? null : clean(columns[c].InnerText);
                    switch (c)
                    {
                        case 0:
                            level.attack.physical = value;
                            break;
                        case 1:
                            level.attack.incantationScaling = isSeal ? scaling : null;
                            break;
                        case 2:
                            level.attack.magic = value;
                            break;
                        case 3:
                            level.attack.fire = value;
                            break;
                        case 4:
                            level.attack.lightning = value;
                            break;
                        case 5:
                            level.attack.holy = value;
                            break;
                        case 6:
                            level.attack.stamina = value;
                            break;
                        case 7:
                            level.scaling.strength = scaling;
                            break;
                        case 8:
                            level.scaling.dexterity = scaling;
                            break;
                        case 9:
                            level.scaling.intelligence = scaling;
                            break;
                        case 10:
                            level.scaling.faith = scaling;
                            break;
                        case 11:
                            level.scaling.arcane = scaling;
                            break;
                        case 13:
                            level.guard.physical = value;
                            break;
                        case 14:
                            level.guard.magic = value;
                            break;
                        case 15:
                            level.guard.fire = value;
                            break;
                        case 16:
                            level.guard.lightning = value;
                            break;
                        case 17:
                            level.guard.holy = value;
                            break;
                        case 18:
                            level.guard.boost = value;
                            break;
                    }
                }
                #endregion

                affinity.levels.Add(level);
            }

            return affinity;
        }

        private async Task<bool> fetchWeapon(string url)
        {

            Weapon? weapon = await db.getWeaponByURL(url);
            if (weapon != null && Common.ParseISO(weapon.updated) > DateTime.Now.AddDays(-2)) return false;

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(await fetchPage(url));

            var nameNode = document.DocumentNode.SelectSingleNode("//*[@id=\"infobox\"]/div/table/tbody/tr[1]/th/h2");
            if (nameNode == null) return false;
            var weightNode = document.DocumentNode.SelectSingleNode("//*[@id=\"infobox\"]/div/table/tbody/tr[7]/td[1]/span");
            if (weightNode == null) weightNode = document.DocumentNode.SelectSingleNode("//*[@id=\"infobox\"]/div/table/tbody/tr[7]/td[1]/div/text()");
            if (weightNode == null) weightNode = document.DocumentNode.SelectSingleNode("//*[@id=\"infobox\"]/div/table/tbody/tr[7]/td[1]/text()");
            if (weightNode == null) return false;

            if (weapon == null) weapon = new();

            weapon.link = url;
            weapon.name = titleCase(nameNode.InnerText);
            weapon.weight = decimal.Parse(clean(weightNode.InnerText));

            #region Type and damage
            var typeNode = document.DocumentNode.SelectSingleNode("//*[@id=\"infobox\"]/div/table/tbody/tr[5]/td[1]/a");
            weapon.type = typeNode != null ? typeNode.InnerText : "Torch";

            var damageNode = document.DocumentNode.SelectSingleNode("//*[@id=\"infobox\"]/div/table/tbody/tr[5]/td[2]/a");
            if (damageNode != null)
            {
                weapon.damage = clean(damageNode.InnerText);
            }
            else
            {
                damageNode = document.DocumentNode.SelectSingleNode("//*[@id=\"infobox\"]/div/table/tbody/tr[5]/td[2]");
                if (damageNode == null)
                {
                    weapon.damage = weapon.type;
                    weapon.type = "Spear";
                }
                else weapon.damage = clean(damageNode.InnerText);
            }
            #endregion

            #region Skill and passive
            var passiveNode = document.DocumentNode.SelectSingleNode("//*[@id=\"infobox\"]/div/table/tbody/tr[7]/td[2]/a[2]");
            if (passiveNode != null)
            {
                var title = passiveNode.GetAttributeValue("title", "");
                if (title != "") weapon.passive = $"{clean(title.Replace("Elden Ring ", ""))} {clean(passiveNode.InnerText, false)}";
            }

            var skillNode = document.DocumentNode.SelectSingleNode("//*[@id=\"infobox\"]/div/table/tbody/tr[6]/td[1]");
            if (skillNode != null && skillNode.InnerText != "--")
            {
                var fpNode = document.DocumentNode.SelectSingleNode("//*[@id=\"infobox\"]/div/table/tbody/tr[6]/td[2]/text()");
                weapon.skill = new()
                {
                    name = skillNode.InnerText,
                    cost = clean(fpNode.InnerText, false)
                };
            }
            #endregion

            #region Affinities
            var affinityTables = document.DocumentNode.SelectNodes("//*[@id=\"wiki-content-block\"]/div[contains(@class, 'tabcontent')][position()>1]/div/table/tbody");

            weapon.affinities.Clear();

            if (affinityTables != null)
            {
                foreach (var table in affinityTables)
                {
                    weapon.affinities.Add(parseWeaponTable(table, weapon.name.ToLower().Contains("seal")));
                }
            }
            else
            {
                var singleTable = document.DocumentNode.SelectNodes("//*[@id=\"wiki-content-block\"]/div/table/tbody");
                if (singleTable != null)
                {
                    if (singleTable.Count == 2) weapon.affinities.Add(parseWeaponTable(singleTable[0], weapon.name.ToLower().Contains("seal")));
                    if (singleTable.Count == 3) weapon.affinities.Add(parseWeaponTable(singleTable[1], weapon.name.ToLower().Contains("seal")));
                }
            }
            #endregion

            #region Requirements
            var requirementsNode = document.DocumentNode.SelectSingleNode("//*[@id=\"infobox\"]/div/table/tbody/tr[4]/td[2]/div");
            if (requirementsNode != null)
            {
                for (var i = 0; i < requirementsNode.ChildNodes.Count; i++)
                {
                    var node = requirementsNode.ChildNodes[i];
                    var cleaned = clean(node.InnerText);
                    if (string.IsNullOrEmpty(cleaned)) continue;

                    var singleLine = Regex.Match(cleaned, @"\w+\s*\d+");

                    string name;
                    int level;

                    if (singleLine.Success)
                    {
                        name = Regex.Replace(cleaned, @"\d*", "");
                        level = int.Parse(Regex.Replace(cleaned, @"\D*", ""));
                    }
                    else
                    {
                        name = cleaned;
                        i++;
                        var nextNode = requirementsNode.ChildNodes[i];
                        level = int.Parse(clean(nextNode.InnerText));
                    }

                    switch (name)
                    {
                        case "Str":
                            weapon.requirements.strength = level;
                            break;
                        case "Dex":
                            weapon.requirements.dexterity = level;
                            break;
                        case "Int":
                            weapon.requirements.intelligence = level;
                            break;
                        case "Fai":
                            weapon.requirements.faith = level;
                            break;
                        case "Arc":
                            weapon.requirements.arcane = level;
                            break;
                    }
                }
            }
            #endregion

            weapon.updated = Common.ToISO();

            await db.setWeapon(weapon);

            return true;
        }

        public async Task fetchWeapons()
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(await fetchPage("https://eldenring.wiki.fextralife.com/Weapons"));

            var links = document.DocumentNode.SelectNodes("//*[@id=\"wiki-content-block\"]/div/div/p/a");

            foreach (var link in links)
            {
                var href = link.GetAttributeValue("href", "");
                if (href == "") continue;
                try
                {
                    bool fetched = await fetchWeapon("https://eldenring.wiki.fextralife.com" + href.Replace(" ", "+"));
                    if (fetched) logger.LogInformation($"Fetched weapon {href}");
                }
                catch (Exception e)
                {
                    logger.LogWarning(e, $"Failed to fetch weapon {href}");
                }
            }
        }
    }
}
