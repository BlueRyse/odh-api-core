﻿using SqlKata;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Helper
{
    public static class PostgresSQLWhereBuilder
    {
        private static readonly string[] _languagesToSearchFor =
            new[] { "de", "it", "en" };

        /// <summary>
        /// Provide title fields as JsonPath
        /// </summary>
        /// <param name="language">
        /// If provided only the fields with the
        /// specified language get returned
        /// </param>
        private static string[] TitleFieldsToSearchFor(string? language) =>
            _languagesToSearchFor.Where(lang =>
                language != null ? lang == language : true
            ).Select(lang =>
                $"Detail.{lang}.Title"
            ).ToArray();

        private static string[] AccoTitleFieldsToSearchFor(string? language) =>
            _languagesToSearchFor.Where(lang =>
                language != null ? lang == language : true
            ).Select(lang =>
                $"AccoDetail.{lang}.Name"
            ).ToArray();

        public static void CheckPassedLanguage(ref string language, IEnumerable<string> availablelanguages)
        {
            language = language.ToLower();

            if (!availablelanguages.Contains(language))
                throw new Exception("passed language not available or passed incorrect string");
        }

        //Return where and Parameters
        [System.Diagnostics.Conditional("TRACE")]
        private static void LogMethodInfo(System.Reflection.MethodBase m, params object?[] parameters)
        {
            var parameterInfo =
                m.GetParameters()
                    .Zip(parameters)
                    .Select((x, _) => (x.First.Name, x.Second));
            Serilog.Log.Debug("{method}({@parameters})", m.Name, parameterInfo);
        }

        //Return Where and Parameters for Activity
        public static Query ActivityWhereExpression(
            this Query query, IReadOnlyCollection<string> languagelist,
            IReadOnlyCollection<string> idlist, IReadOnlyCollection<string> activitytypelist,
            IReadOnlyCollection<string> subtypelist, IReadOnlyCollection<string> difficultylist,
            IReadOnlyCollection<string> smgtaglist, IReadOnlyCollection<string> districtlist,
            IReadOnlyCollection<string> municipalitylist, IReadOnlyCollection<string> tourismvereinlist,
            IReadOnlyCollection<string> regionlist, IReadOnlyCollection<string> arealist, bool distance, int distancemin,
            int distancemax, bool duration, int durationmin, int durationmax, bool altitude, int altitudemin,
            int altitudemax, bool? highlight, bool? activefilter, bool? smgactivefilter, string? searchfilter,
            string? language, string? lastchange, bool filterClosedData)
        {
            LogMethodInfo(
                System.Reflection.MethodBase.GetCurrentMethod()!,
                 "<query>", // not interested in query
                idlist, activitytypelist,
                subtypelist, difficultylist,
                smgtaglist, districtlist,
                municipalitylist, tourismvereinlist,
                regionlist, arealist, distance, distancemin,
                distancemax, duration, durationmin,
                durationmax, altitude, altitudemin,
                altitudemax, highlight, activefilter,
                smgactivefilter, searchfilter,
                language, lastchange
            );

            return query
                .IdUpperFilter(idlist)
                .DistrictFilter(districtlist)
                .LocFilterMunicipalityFilter(municipalitylist)
                .LocFilterTvsFilter(tourismvereinlist)
                .LocFilterRegionFilter(regionlist)
                .AreaFilter(arealist)
                .ActivityTypeFilter(activitytypelist)
                .ActivitySubTypeFilter(subtypelist)
                .DifficultyFilter(difficultylist)
                .DistanceFilter(distance, distancemin, distancemax)
                .DurationFilter(duration, durationmin, durationmax)
                .AltitudeFilter(altitude, altitudemin, altitudemax)
                .HighlightFilter(highlight)
                .ActiveFilter(activefilter)
                .SmgActiveFilter(smgactivefilter)
                .SmgTagFilter(smgtaglist)
                .SearchFilter(TitleFieldsToSearchFor(language), searchfilter)
                .LastChangedFilter(lastchange)
                .When(filterClosedData, q => q.FilterClosedData());
        }

        //Return Where and Parameters for Poi
        public static Query PoiWhereExpression(
            this Query query, IReadOnlyCollection<string> languagelist,
            IReadOnlyCollection<string> idlist, IReadOnlyCollection<string> poitypelist,
            IReadOnlyCollection<string> subtypelist, IReadOnlyCollection<string> smgtaglist,
            IReadOnlyCollection<string> districtlist, IReadOnlyCollection<string> municipalitylist,
            IReadOnlyCollection<string> tourismvereinlist, IReadOnlyCollection<string> regionlist,
            IReadOnlyCollection<string> arealist, bool? highlight, bool? activefilter,
            bool? smgactivefilter, string? searchfilter, string? language, string? lastchange,
            bool filterClosedData)
        {
            LogMethodInfo(
                System.Reflection.MethodBase.GetCurrentMethod()!,
                 "<query>",
                idlist, poitypelist,
                subtypelist, smgtaglist,
                districtlist, municipalitylist,
                tourismvereinlist, regionlist,
                arealist, highlight, activefilter,
                smgactivefilter, searchfilter,
                language, lastchange
            );

            return query
                .IdUpperFilter(idlist)
                .DistrictFilter(districtlist)
                .LocFilterMunicipalityFilter(municipalitylist)
                .LocFilterTvsFilter(tourismvereinlist)
                .LocFilterRegionFilter(regionlist)
                .AreaFilter(arealist)
                .PoiTypeFilter(poitypelist)
                .PoiSubTypeFilter(subtypelist)
                .SmgTagFilter(smgtaglist)
                .HighlightFilter(highlight)
                .ActiveFilter(activefilter)
                .SmgActiveFilter(smgactivefilter)
                .SearchFilter(TitleFieldsToSearchFor(language), searchfilter)
                .LastChangedFilter(lastchange)
                .When(filterClosedData, q => q.FilterClosedData());
        }

        //Return Where and Parameters for Gastronomy
        public static Query GastronomyWhereExpression(
            this Query query, IReadOnlyCollection<string> languagelist,
            IReadOnlyCollection<string> idlist, IReadOnlyCollection<string> dishcodeslist,
            IReadOnlyCollection<string> ceremonycodeslist, IReadOnlyCollection<string> categorycodeslist,
            IReadOnlyCollection<string> facilitycodeslist, IReadOnlyCollection<string> smgtaglist,
            IReadOnlyCollection<string> districtlist, IReadOnlyCollection<string> municipalitylist,
            IReadOnlyCollection<string> tourismvereinlist, IReadOnlyCollection<string> regionlist, bool? activefilter,
            bool? smgactivefilter, string? searchfilter, string? language, string? lastchange,
            bool filterClosedData)
        {
            LogMethodInfo(
                System.Reflection.MethodBase.GetCurrentMethod()!,
                idlist, dishcodeslist,
                ceremonycodeslist, categorycodeslist,
                facilitycodeslist, smgtaglist,
                districtlist, municipalitylist,
                tourismvereinlist, regionlist,
                activefilter,
                smgactivefilter, searchfilter,
                language, lastchange
            );

            return query
                .IdUpperFilter(idlist)
                .DistrictFilter(districtlist)
                .LocFilterMunicipalityFilter(municipalitylist)
                .LocFilterTvsFilter(tourismvereinlist)
                .LocFilterRegionFilter(regionlist)
                .CeremonyCodeFilter(ceremonycodeslist)
                .CategoryCodeFilter(categorycodeslist)
                .CuisineCodeFilter(facilitycodeslist)
                .DishCodeFilter(dishcodeslist)
                .SmgTagFilter(smgtaglist)
                .ActiveFilter(activefilter)
                .SmgActiveFilter(smgactivefilter)
                .SearchFilter(TitleFieldsToSearchFor(language), searchfilter)
                .LastChangedFilter(lastchange)
                .When(filterClosedData, q => q.FilterClosedData());
        }

        //Return Where and Parameters for Activity
        public static Query ODHActivityPoiWhereExpression(
            this Query query, IReadOnlyCollection<string> languagelist,
            IReadOnlyCollection<string> idlist, IReadOnlyCollection<string> typelist, IReadOnlyCollection<string> subtypelist,
            IReadOnlyCollection<string> poitypelist, IReadOnlyCollection<string> sourcelist,
            IReadOnlyCollection<string> smgtaglist, IReadOnlyCollection<string> districtlist,
            IReadOnlyCollection<string> municipalitylist, IReadOnlyCollection<string> tourismvereinlist,
            IReadOnlyCollection<string> regionlist, IReadOnlyCollection<string> arealist, bool? highlight, bool? activefilter, bool? smgactivefilter,
            string? searchfilter, string? language, string? lastchange, bool filterClosedData)
        {
            LogMethodInfo(
                System.Reflection.MethodBase.GetCurrentMethod()!,
                 "<query>", // not interested in query
                idlist, typelist,
                subtypelist, poitypelist, languagelist, sourcelist,
                smgtaglist, districtlist,
                municipalitylist, tourismvereinlist,
                regionlist, arealist, highlight, activefilter,
                smgactivefilter, searchfilter,
                language, lastchange
            );

            return query
                .IdLowerFilter(idlist)
                .DistrictFilter(districtlist)
                .LocFilterMunicipalityFilter(municipalitylist)
                .LocFilterTvsFilter(tourismvereinlist)
                .LocFilterRegionFilter(regionlist)
                .AreaFilter(arealist)
                .ODHActivityPoiTypeFilter(typelist)
                .ODHActivityPoiSubTypeFilter(subtypelist)
                .ODHActivityPoiPoiTypeFilter(subtypelist)
                .SourceFilter(sourcelist)
                .HasLanguageFilter(languagelist)
                .HighlightFilter(highlight)
                .ActiveFilter(activefilter)
                .SmgActiveFilter(smgactivefilter)
                .SmgTagFilter(smgtaglist)
                .SearchFilter(TitleFieldsToSearchFor(language), searchfilter)
                .LastChangedFilter(lastchange)
                .When(filterClosedData, q => q.FilterClosedData());
        }

        //Return Where and Parameters for Article
        public static Query ArticleWhereExpression(
            this Query query, IReadOnlyCollection<string> languagelist,
            IReadOnlyCollection<string> idlist, IReadOnlyCollection<string> typelist, IReadOnlyCollection<string> subtypelist,
            IReadOnlyCollection<string> smgtaglist, bool? highlight, bool? activefilter, bool? smgactivefilter,
            string? searchfilter, string? language, string? lastchange, bool filterClosedData)
        {
            LogMethodInfo(
                System.Reflection.MethodBase.GetCurrentMethod()!,
                 "<query>", // not interested in query
                idlist, typelist,
                subtypelist, languagelist, smgtaglist,
                highlight, activefilter,
                smgactivefilter, searchfilter,
                language, lastchange
            );

            return query
                .IdLowerFilter(idlist)
                .ODHActivityPoiTypeFilter(typelist)
                .ODHActivityPoiSubTypeFilter(subtypelist)
                .ODHActivityPoiPoiTypeFilter(subtypelist)
                .HasLanguageFilter(languagelist)
                .HighlightFilter(highlight)
                .ActiveFilter(activefilter)
                .SmgActiveFilter(smgactivefilter)
                .SmgTagFilter(smgtaglist)
                .SearchFilter(TitleFieldsToSearchFor(language), searchfilter)
                .LastChangedFilter(lastchange)
                .When(filterClosedData, q => q.FilterClosedData());
        }

        //Return Where and Parameters for Event
        public static Query EventWhereExpression(
          this Query query, IReadOnlyCollection<string> languagelist,
          IReadOnlyCollection<string> idlist, IReadOnlyCollection<string> topiclist,
          IReadOnlyCollection<string> typelist, IReadOnlyCollection<string> ranclist,
          IReadOnlyCollection<string> smgtaglist, IReadOnlyCollection<string> districtlist,
          IReadOnlyCollection<string> municipalitylist, IReadOnlyCollection<string> tourismvereinlist,
          IReadOnlyCollection<string> regionlist, IReadOnlyCollection<string> orglist, DateTime? begindate, DateTime? enddate,
          bool? activefilter, bool? smgactivefilter, string? searchfilter,
          string? language, string? lastchange, bool filterClosedData)
        {
            LogMethodInfo(
                System.Reflection.MethodBase.GetCurrentMethod()!,
                 "<query>", // not interested in query
                idlist, topiclist,
                typelist, ranclist,
                smgtaglist, districtlist,
                municipalitylist, tourismvereinlist,
                regionlist, orglist, begindate, enddate,
                activefilter, smgactivefilter, searchfilter,
                language, lastchange
            );

            return query
                .IdUpperFilter(idlist)
                .DistrictFilter(districtlist)
                .LocFilterMunicipalityFilter(municipalitylist)
                .LocFilterTvsFilter(tourismvereinlist)
                .LocFilterRegionFilter(regionlist)
                .EventTopicFilter(topiclist)
                .EventTypeFilter(typelist)
                .EventRancFilter(ranclist)
                .EventOrgFilter(orglist)
                .EventDateFilterEnd(begindate, enddate)
                .EventDateFilterBegin(begindate, enddate)
                .EventDateFilterBeginEnd(begindate, enddate)
                .ActiveFilter(activefilter)
                .SmgActiveFilter(smgactivefilter)
                .SmgTagFilter(smgtaglist)
                .SearchFilter(TitleFieldsToSearchFor(language), searchfilter)
                .LastChangedFilter(lastchange)
                .When(filterClosedData, q => q.FilterClosedData());
        }

        //Return Where and Parameters for Accommodation
        public static Query AccommodationWhereExpression(
            this Query query, IReadOnlyCollection<string> languagelist,
            IReadOnlyCollection<string> idlist, IReadOnlyCollection<string> accotypelist, IReadOnlyCollection<string> categorylist,
            Dictionary<string, bool> featurelist, IReadOnlyCollection<string> featureidlist, IReadOnlyCollection<string> badgelist, Dictionary<string, bool> themelist,
            IReadOnlyCollection<string> boardlist, IReadOnlyCollection<string> smgtaglist, IReadOnlyCollection<string> districtlist,
            IReadOnlyCollection<string> municipalitylist, IReadOnlyCollection<string> tourismvereinlist,
            IReadOnlyCollection<string> regionlist, bool? apartmentfilter, bool? bookable,
            bool altitude, int altitudemin, int altitudemax, bool? activefilter, bool? smgactivefilter,
            string? searchfilter, string? language, string? lastchange, bool filterClosedData)
        {
            LogMethodInfo(
                System.Reflection.MethodBase.GetCurrentMethod()!,
                 "<query>", // not interested in query
                idlist, accotypelist, categorylist,
                featurelist, featureidlist, badgelist, languagelist, themelist, boardlist,
                smgtaglist, districtlist, municipalitylist, tourismvereinlist,
                regionlist, altitude, altitudemin, altitudemax, activefilter,
                smgactivefilter, searchfilter, apartmentfilter, bookable,
                language, lastchange
            );

            return query
                .IdUpperFilter(idlist)
                .DistrictFilter(districtlist)
                .LocFilterMunicipalityFilter(municipalitylist)
                .LocFilterTvsFilter(tourismvereinlist)
                .LocFilterRegionFilter(regionlist)
                .AccoAltitudeFilter(altitude, altitudemin, altitudemax)
                .AccoTypeFilter(accotypelist)
                .AccoCategoryFilter(categorylist)
                .AccoFeatureFilter(featurelist.Where(x => x.Value == true).Select(x => x.Key).ToList())
                .AccoFeatureIdFilter(featureidlist)
                .AccoBadgeFilter(badgelist)
                .AccoThemeFilter(themelist.Where(x => x.Value == true).Select(x => x.Key).ToList())
                .AccoBoardFilter(boardlist)
                .AccoApartmentFilter(apartmentfilter)
                .AccoBoardFilter(boardlist)
                .AccoBookableFilter(bookable)
                // FILTERS Available Marketinggroup, LTSFeature, BookingPortal
                //.HasLanguageFilter(languagelist)
                .ActiveFilter(activefilter)
                .SmgActiveFilter(smgactivefilter)
                .SmgTagFilter(smgtaglist)
                .SearchFilter(AccoTitleFieldsToSearchFor(language), searchfilter)
                .LastChangedFilter(lastchange)
                .When(filterClosedData, q => q.FilterClosedData());
        }

        //Return Where and Parameters for Common
        public static Query CommonWhereExpression(
            this Query query, IReadOnlyCollection<string> languagelist,
            string? searchfilter, bool? visibleinsearch,
            string? language, string? lastchange, bool filterClosedData)
        {
            LogMethodInfo(
                System.Reflection.MethodBase.GetCurrentMethod()!,
                 "<query>", // not interested in query
                searchfilter, language, lastchange
            );

            return query
                .SearchFilter(TitleFieldsToSearchFor(language), searchfilter)
                .LastChangedFilter(lastchange)
                .VisibleInSearchFilter(visibleinsearch)
                .When(filterClosedData, q => q.FilterClosedData());
        }

        //Return Where and Parameters for Wine
        public static Query WineWhereExpression(
            this Query query, IReadOnlyCollection<string> languagelist, IReadOnlyCollection<string> companyid, IReadOnlyCollection<string> wineid,
            string? searchfilter, string? language, string? lastchange, bool filterClosedData)
        {
            LogMethodInfo(
                System.Reflection.MethodBase.GetCurrentMethod()!,
                 "<query>", // not interested in query
                searchfilter, language, lastchange
            );

            return query
                .SearchFilter(TitleFieldsToSearchFor(language), searchfilter)
                .LastChangedFilter(lastchange)
                .CompanyIdFilter(companyid)
                .WineIdFilter(wineid)
                .When(filterClosedData, q => q.FilterClosedData());
        }

        //Return Where and Parameters for WebCamInfo
        public static Query WebCamInfoWhereExpression(
            this Query query, IReadOnlyCollection<string> languagelist,
            IReadOnlyCollection<string> idlist, IReadOnlyCollection<string> sourcelist,
            bool? activefilter, bool? smgactivefilter, string? searchfilter,
            string? language, string? lastchange, bool filterClosedData)
        {
            LogMethodInfo(
                System.Reflection.MethodBase.GetCurrentMethod()!,
                 "<query>", // not interested in query
                idlist, sourcelist,
                activefilter,
                smgactivefilter, searchfilter,
                language, lastchange
            );

            return query
                .IdUpperFilter(idlist)
                .SourceFilter(sourcelist)
                .ActiveFilter(activefilter)
                .SmgActiveFilter(smgactivefilter)
                .SearchFilter(TitleFieldsToSearchFor(language), searchfilter) //TODO here the title is in another field
                .LastChangedFilter(lastchange)
                .When(filterClosedData, q => q.FilterClosedData());
        }

        //Return Where and Parameters for Measuringpoint
        public static Query MeasuringpointWhereExpression(
            this Query query, 
            IReadOnlyCollection<string> idlist, IReadOnlyCollection<string> districtlist,
            IReadOnlyCollection<string> municipalitylist, IReadOnlyCollection<string> tourismvereinlist,
            IReadOnlyCollection<string> regionlist, IReadOnlyCollection<string> arealist,
            bool? activefilter, bool? smgactivefilter, string? searchfilter,
            string? language, string? lastchange, bool filterClosedData)
        {
            LogMethodInfo(
                System.Reflection.MethodBase.GetCurrentMethod()!,
                 "<query>", // not interested in query
                idlist, districtlist, municipalitylist, tourismvereinlist, regionlist,
                arealist, activefilter,
                smgactivefilter, searchfilter,
                language, lastchange
            );

            return query
                .IdUpperFilter(idlist)
                .LocFilterDistrictFilter(districtlist)
                .LocFilterMunicipalityFilter(municipalitylist)
                .LocFilterTvsFilter(tourismvereinlist)
                .LocFilterRegionFilter(regionlist)
                .AreaFilterMeasuringpoints(arealist)
                .ActiveFilter(activefilter)                
                .SmgActiveFilter(smgactivefilter)
                .SearchFilter(TitleFieldsToSearchFor(language), searchfilter) //TODO here the title is in another field
                .LastChangedFilter(lastchange)
                .When(filterClosedData, q => q.FilterClosedData());
        }

        //Return Where and Parameters for Measuringpoint
        public static Query EventShortWhereExpression(
            this Query query, 
            IReadOnlyCollection<string> idlist, IReadOnlyCollection<string> sourcelist,
            IReadOnlyCollection<string> eventlocationlist, IReadOnlyCollection<string> webaddresslist,
            string? activefilter, DateTime? start, DateTime? end, string? searchfilter,
            string? language, string? lastchange, bool filterClosedData, bool getbyrooms = false)
        {
            LogMethodInfo(
                System.Reflection.MethodBase.GetCurrentMethod()!,
                 "<query>", // not interested in query
                idlist, sourcelist, eventlocationlist, webaddresslist,
                activefilter, start, end,
                searchfilter,
                language, lastchange
            );

            return query
                .IdLowerFilter(idlist)
                .SourceFilter(sourcelist)
                .EventShortLocationFilter(eventlocationlist)
                .EventShortWebaddressFilter(webaddresslist)
                .EventShortActiveFilter(activefilter)
                .EventShortDateFilterEnd(start, end, !getbyrooms)
                .EventShortDateFilterBegin(start, end, !getbyrooms)
                .EventShortDateFilterBeginEnd(start, end, !getbyrooms)
                .EventShortDateFilterEndByRoom(start, end, getbyrooms)
                .EventShortDateFilterBeginByRoom(start, end, getbyrooms)
                .EventShortDateFilterBeginEndByRoom(start, end, getbyrooms)
                .SearchFilter(TitleFieldsToSearchFor(language), searchfilter) //TODO here the title is in another field
                .LastChangedFilter(lastchange)
                .When(filterClosedData, q => q.FilterClosedData());
        }

        //Return Where and Parameters for AlpineBits
        public static Query AlpineBitsWhereExpression(
            this Query query,
            IReadOnlyCollection<string> idlist, IReadOnlyCollection<string> sourcelist,
            IReadOnlyCollection<string> accommodationIds, IReadOnlyCollection<string> messagetypelist,
            string requestdate)
        {
            LogMethodInfo(
                System.Reflection.MethodBase.GetCurrentMethod()!,
                 "<query>", // not interested in query
                idlist, sourcelist, accommodationIds, messagetypelist, requestdate
            );

            return query
                .IdIlikeFilter(idlist)
                .SourceFilter(sourcelist)
                .AlpineBitsMessageFilter(messagetypelist)
                .AlpineBitsAccommodationIdFilter(accommodationIds);
        }
    }
}
