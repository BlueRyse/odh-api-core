﻿using AspNetCore.CacheOutput;
using DataModel;
using Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OdhApiCore.Filters;
using OdhApiCore.Responses;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OdhApiCore.Controllers
{    
    [EnableCors("CorsPolicy")]
    //Do not show in Swagger for now
    [ApiExplorerSettings(IgnoreApi = true)]
    [NullStringParameterActionFilter]
    public class TagController : OdhController
    {
        public TagController(IWebHostEnvironment env, ISettings settings, ILogger<TagController> logger, QueryFactory queryFactory)
            : base(env, settings, logger, queryFactory)
        {
        }

        #region SWAGGER Exposed API

        /// <summary>
        /// GET Tag List
        /// </summary>
        /// <param name="validforentity">Filter on Tags valid on Entities (accommodation, activity, poi, odhactivitypoi, package, gastronomy, event, article, common .. etc..)</param>
        /// <param name="mainentity">Filter on Tags with MainEntity set to (accommodation, activity, poi, odhactivitypoi, package, gastronomy, event, article, common .. etc..)</param>
        /// <param name="displayascategory">true = returns only Tags which are marked as DisplayAsCategory true</param>
        /// <param name="language">Language field selector, displays data and fields available in the selected language (default:'null' all languages are displayed)</param>
        /// <param name="localizationlanguage">here for Compatibility Reasons, replaced by language parameter</param>
        /// <param name="source">Source Filter (possible Values: 'lts','idm), (default:'null')</param>
        /// <param name="fields">Select fields to display, More fields are indicated by separator ',' example fields=Id,Active,Shortname (default:'null' all fields are displayed). <a href="https://github.com/noi-techpark/odh-docs/wiki/Common-parameters%2C-fields%2C-language%2C-searchfilter%2C-removenullvalues%2C-updatefrom#fields" target="_blank">Wiki fields</a></param>
        /// <param name="searchfilter">String to search for, Title in all languages are searched, (default: null) <a href="https://github.com/noi-techpark/odh-docs/wiki/Common-parameters%2C-fields%2C-language%2C-searchfilter%2C-removenullvalues%2C-updatefrom#searchfilter" target="_blank">Wiki searchfilter</a></param>
        /// <param name="rawfilter"><a href="https://github.com/noi-techpark/odh-docs/wiki/Using-rawfilter-and-rawsort-on-the-Tourism-Api#rawfilter" target="_blank">Wiki rawfilter</a></param>
        /// <param name="rawsort"><a href="https://github.com/noi-techpark/odh-docs/wiki/Using-rawfilter-and-rawsort-on-the-Tourism-Api#rawsort" target="_blank">Wiki rawsort</a></param>
        /// <param name="removenullvalues">Remove all Null values from json output. Useful for reducing json size. By default set to false. Documentation on <a href='https://github.com/noi-techpark/odh-docs/wiki/Common-parameters,-fields,-language,-searchfilter,-removenullvalues,-updatefrom#removenullvalues' target="_blank">Opendatahub Wiki</a></param>        
        /// <returns>Collection of Tag Objects</returns>        
        /// <response code="200">List created</response>
        /// <response code="400">Request Error</response>
        /// <response code="500">Internal Server Error</response>
        [ProducesResponseType(typeof(IEnumerable<TagLinked>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [OdhCacheOutput(ClientTimeSpan = 0, ServerTimeSpan = 3600, CacheKeyGenerator = typeof(CustomCacheKeyGenerator))]
        [HttpGet, Route("Tag")]
        //[Authorize(Roles = "DataReader,CommonReader,AccoReader,ActivityReader,PoiReader,ODHPoiReader,PackageReader,GastroReader,EventReader,ArticleReader")]
        public async Task<IActionResult> GetTagAsync(
            uint? pagenumber = null,
            PageSize pagesize = null!,
            string? language = null,
            string? validforentity = null,
            string? mainentity = null,
            bool? displayascategory = null,
            string? source = null,
            [ModelBinder(typeof(CommaSeparatedArrayBinder))]
            string[]? fields = null,
            string? searchfilter = null,
            string? rawfilter = null,
            string? rawsort = null,
            string? localizationlanguage = null,  //TODO ignore this in swagger
            bool removenullvalues = false,
            CancellationToken cancellationToken = default)
        {
            //Compatibility
            if (String.IsNullOrEmpty(language) && !String.IsNullOrEmpty(localizationlanguage))
                language = localizationlanguage;


            return await Get(pagenumber, pagesize, language, mainentity, validforentity, displayascategory, source, fields: fields ?? Array.Empty<string>(), 
                  searchfilter, rawfilter, rawsort, removenullvalues: removenullvalues,
                    cancellationToken);           
        }

        /// <summary>
        /// GET Tag Single
        /// </summary>
        /// <param name="id">ID of the Tag</param>
        /// <param name="language">Language field selector, displays data and fields available in the selected language (default:'null' all languages are displayed)</param>
        /// <param name="fields">Select fields to display, More fields are indicated by separator ',' example fields=Id,Active,Shortname (default:'null' all fields are displayed). <a href="https://github.com/noi-techpark/odh-docs/wiki/Common-parameters%2C-fields%2C-language%2C-searchfilter%2C-removenullvalues%2C-updatefrom#fields" target="_blank">Wiki fields</a></param>
        /// <param name="removenullvalues">Remove all Null values from json output. Useful for reducing json size. By default set to false. Documentation on <a href='https://github.com/noi-techpark/odh-docs/wiki/Common-parameters,-fields,-language,-searchfilter,-removenullvalues,-updatefrom#removenullvalues' target="_blank">Opendatahub Wiki</a></param>        
        /// <returns>TagLinked Object</returns>
        /// <response code="200">Object created</response>
        /// <response code="400">Request Error</response>
        /// <response code="500">Internal Server Error</response>
        [ProducesResponseType(typeof(TagLinked), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet, Route("Tag/{id}", Name = "SingleTag")]
        //[Authorize(Roles = "DataReader,CommonReader,AccoReader,ActivityReader,PoiReader,ODHPoiReader,PackageReader,GastroReader,EventReader,ArticleReader")]
        public async Task<IActionResult> GetTagSingle(uint? pagenumber, int? pagesize, string id,
            string? language = null,
            [ModelBinder(typeof(CommaSeparatedArrayBinder))]
            string[]? fields = null,
            string? localizationlanguage = null,   //TODO ignore this in swagger
            bool removenullvalues = false,
            CancellationToken cancellationToken = default)
        {
            //Compatibility
            if (String.IsNullOrEmpty(language) && !String.IsNullOrEmpty(localizationlanguage))
                language = localizationlanguage;

            return await GetSingle(id, language, fields: fields ?? Array.Empty<string>(), removenullvalues: removenullvalues, cancellationToken);
        }

        #endregion

        #region GETTER

        private Task<IActionResult> Get(
            uint? pagenumber, int? pagesize, 
            string? language, string? maintype, string? validforentity, bool? displayascategory, string? source, string[] fields,
            string? searchfilter, string? rawfilter, string? rawsort, bool removenullvalues,
            CancellationToken cancellationToken)
        {
            return DoAsyncReturn(async () =>
            {
                var validforentitytypeslist = (validforentity ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);
                var maintypeslist = (maintype ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);
                var sourcelist = Helper.CommonListCreator.CreateIdList(source);

                var query = 
                    QueryFactory.Query()
                    .SelectRaw("data")
                    .From("tags")
                    .ODHTagWhereExpression(
                        languagelist: new List<string>(), 
                        mainentitylist: maintypeslist,
                        validforentitylist: validforentitytypeslist,
                        sourcelist: sourcelist,
                        displayascategory: displayascategory,
                        searchfilter: searchfilter,
                        language: language,
                        filterClosedData: FilterClosedData
                        )
                    .ApplyRawFilter(rawfilter)
                    .ApplyOrdering(new PGGeoSearchResult() { geosearch = false }, rawsort, "data #>>'\\{MainEntity\\}', data#>>'\\{Shortname\\}'");

            var fieldsTohide = FieldsToHide;

                if (pagenumber != null)
                {

                    // Get paginated data
                    var data =
                        await query
                            .PaginateAsync<JsonRaw>(
                                page: (int)pagenumber,
                                perPage: pagesize ?? 25);

                    var dataTransformed =
                        data.List.Select(
                            raw => raw.TransformRawData(language, fields, checkCC0: FilterCC0License, filterClosedData: FilterClosedData, filteroutNullValues: removenullvalues, urlGenerator: UrlGenerator, fieldstohide: fieldsTohide)
                        );

                    uint totalpages = (uint)data.TotalPages;
                    uint totalcount = (uint)data.Count;

                    return ResponseHelpers.GetResult(
                        (uint)pagenumber,
                        totalpages,
                        totalcount,
                        null,
                        dataTransformed,
                        Url);
                }
                else
                {
                    var data = await query.GetAsync<JsonRaw>();

                    return data.Select(raw => raw.TransformRawData(language, fields, checkCC0: FilterCC0License, filterClosedData: FilterClosedData, filteroutNullValues: removenullvalues, urlGenerator: UrlGenerator, fieldstohide: fieldsTohide));
                }
            });
        }      

        private Task<IActionResult> GetSingle(string id, string? language, string[] fields, bool removenullvalues, CancellationToken cancellationToken)
        {
            return DoAsyncReturn(async () =>
            {
                var data = await QueryFactory.Query("tags")
                    .Select("data")
                    .Where("id", id.ToLower())
                    .When(FilterClosedData, q => q.FilterClosedData())
                    .FirstOrDefaultAsync<JsonRaw>();

                var fieldsTohide = FieldsToHide;

                return data?.TransformRawData(language, fields, checkCC0: FilterCC0License, filterClosedData: FilterClosedData, filteroutNullValues: removenullvalues, urlGenerator: UrlGenerator, fieldstohide: fieldsTohide);
            });
        }

        #endregion

        #region POST PUT DELETE

        /// <summary>
        /// POST Insert new Tag
        /// </summary>
        /// <param name="tag">Tag Object</param>
        /// <returns>Http Response</returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(Roles = "DataWriter,DataCreate,TagManager,TagCreate")]
        [HttpPost, Route("Tag")]
        public Task<IActionResult> Post([FromBody] TagLinked tag)
        {
            return DoAsyncReturn(async () =>
            {
                tag.Id = !String.IsNullOrEmpty(tag.Id) ? tag.Id.ToUpper() : "noId";
                return await UpsertData<TagLinked>(tag, "tags");
            });
        }

        /// <summary>
        /// PUT Modify existing Tag
        /// </summary>
        /// <param name="id">Tag Id</param>
        /// <param name="tag">Tag Object</param>
        /// <returns>Http Response</returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(Roles = "DataWriter,DataModify,TagManager,TagModify")]
        [HttpPut, Route("Tag/{id}")]
        public Task<IActionResult> Put(string id, [FromBody] TagLinked tag)
        {
            return DoAsyncReturn(async () =>
            {
                tag.Id = id.ToUpper();
                return await UpsertData<TagLinked>(tag, "tags");
            });
        }

        /// <summary>
        /// DELETE Tag by Id
        /// </summary>
        /// <param name="id">Tag Id</param>
        /// <returns>Http Response</returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(Roles = "DataWriter,DataDelete,TagManager,TagDelete")]
        [HttpDelete, Route("Tag/{id}")]
        public Task<IActionResult> Delete(string id)
        {
            return DoAsyncReturn(async () =>
            {
                id = id.ToUpper();
                return await DeleteData(id, "tags");
            });
        }


        #endregion
    }


}