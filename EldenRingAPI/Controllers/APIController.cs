﻿using EldenRingAPI.Interfaces;
using EldenRingAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EldenRingAPI.Controllers
{
    [Produces("application/json")]
    public class APIController : BaseController
    {
        private readonly IAPIService api;

        public APIController(IAPIService api)
        {
            this.api = api;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        [Route("/")]
        public void redirect()
        {
            Response.Redirect("/swagger"); // Temporarily redirect to swagger until landing page is made
        }

        [SwaggerOperation("Get weapons", "Get an array of every weapon")]
        [SwaggerResponse(200, "Success", typeof(IResponse<Results<Weapon>>))]
        [HttpGet]
        [Route("/weapons")]
        public async Task<JsonResult> getWeapons(int page = 1, int perPage = 15)
        {
            return HandleResponse(await api.getWeapons(page, perPage));
        }

        [SwaggerOperation("Get a weapon", "Get the details of a weapon")]
        [SwaggerResponse(200, "Success", typeof(IResponse<Weapon>))]
        [HttpGet]
        [Route("/weapons/{id}")]
        public async Task<JsonResult> getWeapon(string id)
        {
            return HandleResponse(await api.getWeapon(id));
        }
    }
}
