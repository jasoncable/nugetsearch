using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NuGetSearch.Models;
using Nest;
using Elasticsearch;
using Elasticsearch.Net;

namespace NuGetSearch.Repositories
{
    public static class NuGetSearchMainRepo
    {
        public static NuGetSearchMainSearchResult DoSearch(ElasticClient client, NuGetSearchMainSearchRequest request)
        {
            NuGetSearchMainSearchResult result = new NuGetSearchMainSearchResult();
            result.SearchInput = request.SearchString;
            result.Page = request.Page;

            //var searchResult = _repo.Client.Search<NuGetSearchMain>(s =>
            //    s.Query(q =>
            //          q.Match(m =>
            //                m.Field(f => f.Name)
            //                .Query(model.SearchInput)
            //                 )
            //           )
            //    );

            //var searchResult1 = _repo.Client.Search<NuGetSearchMain>(s =>
            //s.Query(q =>
            //     q.CommonTerms(c => c
            //         .Field(p => p.Name)
            //         .Analyzer("standard")
            //         .Boost(1.1)
            //         .CutoffFrequency(0.001)
            //         .HighFrequencyOperator(Operator.And)
            //         .LowFrequencyOperator(Operator.Or)
            //         .MinimumShouldMatch(1)
            //         .Name("named_query")
            //         .Query(model.SearchInput)
            //       )
            //      ).Take(20).From(0)
            //);

            //            return Client.Search<NuGetSearchMain>(s =>
            //               s.Query(q =>
            //                     q
            //                   .MultiMatch(c => c
            //                     .Fields(f => f.Fields("name", "description", "summary", "title", "releaseNotes"))
            //                     .Query(query)
            //                     .Analyzer("standard")
            //                     .Boost(1.1)
            //                     .Slop(2)
            //                     .Fuzziness(Fuzziness.Auto)
            //                     .PrefixLength(2)
            //                     .MaxExpansions(2)
            //                     .Operator(Operator.Or)
            //                     .MinimumShouldMatch(2)
            //                     .FuzzyRewrite(MultiTermQueryRewrite.ConstantScoreBoolean)
            //                     .TieBreaker(1.1)
            //                     .CutoffFrequency(0.001)
            //                     .Lenient()
            //                     .ZeroTermsQuery(ZeroTermsQuery.All)
            //                     .Name("named_query")
            //                  )).Size(20).From(page * 20));

            var query = client.Search<NuGetSearchMain>(s =>
                s.Query(q => GetQuery(q, request.SearchString) )
                .Size(request.PageSize)
                .From(request.Page * request.PageSize)
            );

            result.Count = query.HitsMetadata.Total > request.MaxResults ? request.MaxResults : (int)query.HitsMetadata.Total;
            result.QueryTime = TimeSpan.FromMilliseconds(query.Took);
            result.Data = query.Documents.ToList();

            return result;
        }

        public static QueryContainer GetQuery(QueryContainerDescriptor<NuGetSearchMain> q, string searchString)
        {
            return q.Term(t =>
                       t.Name.Suffix("keyword"), searchString, 1.6)
                   ||
                   q.Match(m =>
                       m.Field(f => f.Name)
                           .Query(searchString)
                           .Boost(1.5))
                   ||
                   q.Match(m =>
                       m.Field(f => f.NameSplit)
                           .Boost(1.41)
                           .Query(searchString)
                   )
                   ||
                   q.Match(m =>
                       m.Field(f => f.NameSplitCamel)
                           .Boost(1.4)
                           .Query(searchString)
                   )
                   ||
                   q.Match(m =>
                       m.Field(f => f.Title.Suffix("stop"))
                           .Query(searchString)
                           .Analyzer("stop")
                           .Boost(1.3))
                   ||
                   q.Match(m =>
                       m.Field(f => f.Tags)
                           .Query(searchString)
                           .Boost(1.25))
                   ||
                   q.Match(m =>
                       m.Field(f => f.Title)
                           .Query(searchString)
                           .Analyzer("stop")
                           .Boost(1.26))
                   ||
                   q.Match(m =>
                       m.Field(f => f.Description)
                           .Query(searchString)
                           .Analyzer("stop")
                           .Boost(1.25))
                   ||
                   q.Match(m =>
                       m.Field(f => f.Summary)
                           .Query(searchString)
                           .Analyzer("stop")
                           .Boost(1.24))
                   ||
                   q.Match(m =>
                       m.Field(f => f.Authors)
                           .Query(searchString)
                           .Boost(1.1));
        }

        public static CreateIndexDescriptor CreateIndex(string indexName)
        {
            CreateIndexDescriptor d = new CreateIndexDescriptor(indexName);

            d.Settings(
                    y => y.NumberOfShards(3)
                )
                .Mappings(ms =>
                    ms.Map<NuGetSearchMain>(m =>
                        m.Properties(p => p
                            .Text(t => t
                                .Name(n => n.Name)
                                .Fields(ff => ff
                                    .Keyword(k => k
                                        .Name("keyword")
                                        .IgnoreAbove(256)
                                    )
                                )
                            )
                            .Text(t => t
                                .Name(n => n.NameSplit)
                            )
                            .Text(t => t
                                .Name(n => n.NameSplitCamel)
                            )
                            .Text(t => t
                                .Name(n => n.Authors)
                                .Fields(ff => ff
                                    .Keyword(k => k
                                        .Name("keyword")
                                        .IgnoreAbove(256)
                                    )
                                )
                            )
                            .Text(t => t
                                .Name(n => n.Description)
                                .Fields(ff => ff
                                    .Text(tt => tt
                                        .Name("stop")
                                        .Analyzer("stop")
                                    )
                                )
                            )

                            .Text(t => t
                                .Name(n => n.IconUrl)
                            )
                            .Text(t => t
                                .Name(n => n.ProjectUrl)
                            )
                            .Text(t => t
                                .Name(n => n.ReleaseNotes)
                                .Fields(ff => ff
                                    .Text(tt => tt
                                        .Name("stop")
                                        .Analyzer("stop")
                                    )
                                )
                            )
                            .Text(t => t
                                .Name(n => n.Tags)

                            )
                            .Text(t => t
                                .Name(n => n.Title)
                                .Fields(ff => ff
                                    .Text(tt => tt
                                        .Name("stop")
                                        .Analyzer("stop")
                                    )
                                )
                            )

                            .Date(dd => dd
                                .Name(n => n.CommitTimeStamp)
                            )

                            .Object<NuGetSearchMainDependencyGroup>(o => o
                                .Name(n => n.DependencyGroups)
                                .Properties(pp => pp
                                    .Text(tt => tt
                                        .Name(nnn => nnn.TargetFramework)
                                        .Fields(ff => ff
                                            .Keyword(kk => kk
                                                .Name("keyword")
                                                .IgnoreAbove(256)
                                            )
                                        )
                                    )
                                    .Object<NuGetSearchMainDependency>(o2 => o2
                                        .Name(nnnn => nnnn.Dependencies)
                                        .Properties(pppp => pppp
                                            .Text(tttt => tttt
                                                .Name(nnnnn => nnnnn.Name)
                                                .Fields(fffff => fffff
                                                    .Keyword(kkkkk => kkkkk
                                                        .Name("keyword")
                                                        .IgnoreAbove(256)
                                                    )
                                                )
                                            )
                                            .Text(tttt => tttt
                                                .Name(nnnnn => nnnnn.VersionRange)
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    )
                );
            return d;
        }

        //public static CreateIndexDescriptor CreateIndex(string indexName)
        //{
        //    CreateIndexDescriptor d = new CreateIndexDescriptor(indexName);

        //    d.Settings(
        //            y => y.NumberOfShards(3)
        //        )
        //        .Mappings(ms =>
        //            ms.Map<NuGetSearchMain>(m =>
        //                m.AutoMap()
        //            )
        //        );

        //    return d;
        //}

    }
}
