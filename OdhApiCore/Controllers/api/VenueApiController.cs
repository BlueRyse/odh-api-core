﻿using DataModel;
using Helper;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OdhApiCore.Responses;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OdhApiCore.Controllers
{
    /// <summary>
    /// Venue Api (data provided by LTS Venues) SOME DATA Available as OPENDATA
    /// </summary>
    [EnableCors("CorsPolicy")]
    [NullStringParameterActionFilter]
    public class VenueController : OdhController
    {
        // Only for test purposes

        public VenueController(IWebHostEnvironment env, ISettings settings, ILogger<VenueController> logger, QueryFactory queryFactory)
            : base(env, settings, logger, queryFactory)
        {
        }

        #region SWAGGER Exposed API

        /// <summary>
        /// GET Venue List
        /// </summary>
        /// <param name="pagenumber">Pagenumber, (default:1)</param>
        /// <param name="pagesize">Elements per Page (max 1024), (default:10)</param>
        /// <param name="seed">Seed '1 - 10' for Random Sorting, '0' generates a Random Seed, not provided disables Random Sorting, (default:'null') </param>
        /// <param name="idlist">IDFilter (Separator ',' List of Venue IDs), (default:'null')</param>
        /// <param name="locfilter">Locfilter (Separator ',' possible values: mta + METAREGIONID = (Filter by Metaregion), reg + REGIONID = (Filter by Region), tvs + TOURISMVEREINID = (Filter by Tourismverein), mun + MUNICIPALITYID = (Filter by Municipality), fra + FRACTIONID = (Filter by Fraction)), (default:'null' = disabled)</param>
        /// <param name="categoryfilter">Venue Category Filter (BITMASK) (Separator ',' List of Venuetype Bitmasks, refer to api/VenueTypes type:category), (default:'null')</param>
        /// <param name="featurefilter">Venue Features Filter (BITMASK) (Separator ',' List of Venuetype Bitmasks, refer to api/VenueTypes type:feature), (default:'null')</param>
        /// <param name="setuptypefilter">Venue SetupType Filter (BITMASK) (Separator ',' List of Venuetype Bitmasks, refer to api/VenueTypes type:seatType), (default:'null')</param>
        /// <param name="source">Source Filter(String, ), (default:'null')</param>        
        /// <param name="capacityfilter">Capacity Range Filter (Separator ',' example Value: 50,100 All Venues with rooms from 50 to 100 people), (default:'null')</param>
        /// <param name="roomcountfilter">Room Count Range Filter (Separator ',' example Value: 2,5 All Venues with 2 to 5 rooms), (default:'null')</param>
        /// <param name="odhtagfilter">ODH Taglist Filter (refers to Array SmgTags)(String, Separator ',' more Tags possible, available Tags reference to 'api/SmgTag/ByMainEntity/SmgPoi'), (default:'null')</param>        
        /// <param name="active">Active Venue Filter (possible Values: 'true' only Active Gastronomies, 'false' only Disabled Venues</param>
        /// <param name="odhactive">ODH Active (Published) Venue Filter (possible Values: 'true' only published Venue, 'false' only not published Venue, (default:'null')</param>        
        /// <param name="latitude">GeoFilter Latitude Format: '46.624975', 'null' = disabled, (default:'null')</param>
        /// <param name="longitude">GeoFilter Longitude Format: '11.369909', 'null' = disabled, (default:'null')</param>
        /// <param name="radius">Radius to Search in KM. Only Object withhin the given point and radius are returned and sorted by distance. Random Sorting is disabled if the GeoFilter Informations are provided, (default:'null')</param>
        /// <param name="updatefrom">Returns data changed after this date Format (yyyy-MM-dd), (default: 'null')</param>
        /// <param name="fields">Select fields to display, More fields are indicated by separator ',' example fields=Id,Active,Shortname. Select also Dictionary fields, example Detail.de.Title, or Elements of Arrays example ImageGallery[0].ImageUrl. (default:'null' all fields are displayed)</param>
        /// <param name="language">Language field selector, displays data and fields available in the selected language (default:'null' all languages are displayed)</param>
        /// <param name="searchfilter">String to search for, Title in all languages are searched, (default: null)</param>
        /// <returns>Collection of DDVenue Objects</returns>    
        /// <response code="200">List created</response>
        /// <response code="400">Request Error</response>
        /// <response code="500">Internal Server Error</response>
        [ProducesResponseType(typeof(JsonResult<DDVenue>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //[Authorize(Roles = "DataReader,VenueReader")]
        //[Authorize]
        [HttpGet, Route("Venue")]
        public async Task<IActionResult> GetVenueList(
            string? language = null,
            uint pagenumber = 1,
            PageSize pagesize = null!,
            string? categoryfilter = null,
            string? capacityfilter = null,
            string? roomcountfilter = null,
            string? idlist = null,
            string? locfilter = null,
            string? featurefilter = null,
            string? setuptypefilter = null,
            string? odhtagfilter = null,
            string? source = null,
            LegacyBool active = null!,
            LegacyBool odhactive = null!,
            string? updatefrom = null,
            string? seed = null,
            string? latitude = null,
            string? longitude = null,
            string? radius = null,
            [ModelBinder(typeof(CommaSeparatedArrayBinder))]
            string[]? fields = null,
            string? searchfilter = null,
            string? rawfilter = null,
            string? rawsort = null,
            CancellationToken cancellationToken = default)
        {
            var geosearchresult = Helper.GeoSearchHelper.GetPGGeoSearchResult(latitude, longitude, radius);

            return await GetFiltered(
                    fields: fields ?? Array.Empty<string>(), language: language, pagenumber: pagenumber,
                    pagesize: pagesize, idfilter: idlist, categoryfilter: categoryfilter, capacityfilter: capacityfilter,
                    searchfilter: searchfilter, locfilter: locfilter, roomcountfilter: roomcountfilter,
                    featurefilter: featurefilter, setuptypefilter: setuptypefilter, sourcefilter: source,
                    active: active, smgactive: odhactive, smgtags: odhtagfilter, seed: seed, lastchange: updatefrom,
                    geosearchresult: geosearchresult, rawfilter: rawfilter, rawsort: rawsort, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// GET Venue Single
        /// </summary>
        /// <param name="id">ID of the Venue</param>
        /// <returns>DDVenue Object</returns>
        /// <response code="200">Object created</response>
        /// <response code="400">Request Error</response>
        /// <response code="500">Internal Server Error</response>
        /// //[Authorize(Roles = "DataReader,VenueReader")]
        [ProducesResponseType(typeof(DDVenue), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet, Route("Venue/{id}", Name = "SingleVenue")]
        public async Task<IActionResult> GetVenueSingle(
            string id,
            string? language,
            [ModelBinder(typeof(CommaSeparatedArrayBinder))]
            string[]? fields = null,
            CancellationToken cancellationToken = default)
        {
            return await GetSingle(id, language, fields: fields ?? Array.Empty<string>(), cancellationToken);
        }

        /// <summary>
        /// GET Venue Types List
        /// </summary>
        /// <returns>Collection of VenueTypes Object</returns>
        /// <response code="200">List created</response>
        /// <response code="400">Request Error</response>
        /// <response code="500">Internal Server Error</response>
        //[CacheOutputUntilToday(23, 59)]
        [ProducesResponseType(typeof(IEnumerable<VenueType>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //[Authorize(Roles = "DataReader,VenueReader")]
        [HttpGet, Route("VenueTypes")]
        public async Task<IActionResult> GetAllVenueTypesListAsync(CancellationToken cancellationToken)
        {
            return await GetVenueTypesListAsync(cancellationToken);
        }

        /// <summary>
        /// GET Venue Types Single
        /// </summary>
        /// <returns>VenueTypes Object</returns>
        /// <response code="200">List created</response>
        /// <response code="400">Request Error</response>
        /// <response code="500">Internal Server Error</response>
        //[CacheOutputUntilToday(23, 59)]
        [ProducesResponseType(typeof(VenueType), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //[Authorize(Roles = "DataReader,VenueReader")]
        [HttpGet, Route("VenueTypes/{id}", Name = "SingleVenueTypes")]
        public async Task<IActionResult> GetAllVenueTypesSingleAsync(string id, CancellationToken cancellationToken)
        {
            return await GetVenueTypesSingleAsync(id, cancellationToken);
        }

        #endregion

        #region GETTER

        private Task<IActionResult> GetFiltered(
          string[] fields, string? language, uint pagenumber, int? pagesize, string? idfilter, string? categoryfilter, string? capacityfilter,
          string? searchfilter, string? locfilter, string? roomcountfilter, string? featurefilter, string? setuptypefilter,
          string? sourcefilter, bool? active, bool? smgactive, string? smgtags, string? seed, string? lastchange, 
          PGGeoSearchResult geosearchresult, string? rawfilter, string? rawsort,
          CancellationToken cancellationToken)
        {
            return DoAsyncReturn(async () =>
            {
                VenueHelper myvenuehelper = await VenueHelper.CreateAsync(
                    QueryFactory, idfilter, categoryfilter, featurefilter, setuptypefilter, locfilter, capacityfilter, roomcountfilter,
                    language, sourcefilter, active, smgactive, smgtags, lastchange,
                    cancellationToken);

                var query =
                    QueryFactory.Query()
                        .SelectRaw("data")
                        .From("venues")
                        .VenueWhereExpression(
                            languagelist: new List<string>(), idlist: myvenuehelper.idlist, categorylist: myvenuehelper.categorylist,
                            featurelist: myvenuehelper.featurelist, setuptypelist: myvenuehelper.setuptypelist,
                            smgtaglist: myvenuehelper.odhtaglist, districtlist: myvenuehelper.districtlist,
                            municipalitylist: myvenuehelper.municipalitylist, tourismvereinlist: myvenuehelper.tourismvereinlist,
                            regionlist: myvenuehelper.regionlist, sourcelist: myvenuehelper.sourcelist,
                            capacity: myvenuehelper.capacity, capacitymin: myvenuehelper.capacitymin, capacitymax: myvenuehelper.capacitymax,
                            roomcount: myvenuehelper.roomcount, roomcountmin: myvenuehelper.roomcountmin, roomcountmax: myvenuehelper.roomcountmax,
                            activefilter: myvenuehelper.active, smgactivefilter: myvenuehelper.smgactive,
                            searchfilter: searchfilter, language: language, lastchange: myvenuehelper.lastchange, 
                            filterClosedData: FilterClosedData)
                        .ApplyRawFilter(rawfilter)
                        .ApplyOrdering(ref seed, geosearchresult, rawsort);

                // Get paginated data
                var data =
                    await query
                        .PaginateAsync<JsonRaw>(
                            page: (int)pagenumber,
                            perPage: (int)pagesize);

                var dataTransformed =
                    data.List.Select(
                        raw => raw.TransformRawData(language, fields, checkCC0: FilterCC0License, filterClosedData: FilterClosedData, urlGenerator: UrlGenerator, userroles: UserRolesList)
                    );

                uint totalpages = (uint)data.TotalPages;
                uint totalcount = (uint)data.Count;

                return ResponseHelpers.GetResult(
                    pagenumber,
                    totalpages,
                    totalcount,
                    seed,
                    dataTransformed,
                    Url);
            });
        }

        private Task<IActionResult> GetSingle(string id, string? language, string[] fields, CancellationToken cancellationToken)
        {
            return DoAsyncReturn(async () =>
            {
                var query =
                    QueryFactory.Query("venues")
                        .Select("data")
                        .Where("id", id)
                        .When(FilterClosedData, q => q.FilterClosedData());

                var data = await query.FirstOrDefaultAsync<JsonRaw?>();

                return data?.TransformRawData(language, fields, checkCC0: FilterCC0License, filterClosedData: FilterClosedData, urlGenerator: UrlGenerator, userroles: UserRolesList);
            });
        }

        #endregion

        #region CUSTOM METHODS

        private Task<IActionResult> GetVenueTypesListAsync(CancellationToken cancellationToken)
        {
            return DoAsyncReturn(async () =>
            {
                var query =
                    QueryFactory.Query("venuetypes")
                        .SelectRaw("data")
                        .When(FilterClosedData, q => q.FilterClosedData());

                var data = await query.GetAsync<JsonRaw?>();

                return data;
            });
        }

        /// <summary>
        /// GET Venue Types Single
        /// </summary>
        /// <returns>VenueTypes Object</returns>
        private Task<IActionResult> GetVenueTypesSingleAsync(string id, CancellationToken cancellationToken)
        {
            return DoAsyncReturn(async () =>
            {
                var query =
                    QueryFactory.Query("venuetypes")
                        .Select("data")
                         //.WhereJsonb("Key", "ilike", id)
                         .Where("id", id.ToUpper())
                        .When(FilterClosedData, q => q.FilterClosedData());
                //.Where("Key", "ILIKE", id);

                var data = await query.FirstOrDefaultAsync<JsonRaw?>();

                return data;
            });
        }

        #endregion
    }
}