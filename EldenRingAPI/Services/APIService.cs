using EldenRingAPI.Interfaces;
using System.Net;

namespace EldenRingAPI.Services
{
    public class APIService : IAPIService
    {
        private readonly Database db;
        private readonly ILogger logger;

        public APIService(Database db, ILogger<APIService> logger)
        {
            this.db = db;
            this.logger = logger;
        }

        public async Task<Response> getWeapons(int page, int perPage)
        {
            try
            {
                if (page < 1) page = 1;
                if (perPage < 1) perPage = 1;
                var weapons = await db.getWeapons(page, perPage);

                return new Response(HttpStatusCode.OK, "Ok", weapons);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error fetching all weapons");
                return new Response(HttpStatusCode.InternalServerError, "Unable to fetch weapons");
            }
        }

        public async Task<Response> getWeapon(string id)
        {
            try
            {
                var weapon = await db.getWeaponByID(id);

                return new Response(HttpStatusCode.OK, "Ok", weapon);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error fetching all weapon");
                return new Response(HttpStatusCode.InternalServerError, "Unable to fetch weapon");
            }
        }
    }
}
