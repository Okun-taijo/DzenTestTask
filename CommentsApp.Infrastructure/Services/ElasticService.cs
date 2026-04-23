using CommentsApp.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Nest;

namespace CommentsApp.Infrastructure.Services
{
    public class ElasticService : IElasticService
    {
        private readonly IElasticClient _client;
        private const string IndexName = "comments";

        public ElasticService(IConfiguration config)
        {
            var uri = config["Elastic:Uri"] ?? "http://localhost:9200";
            var settings = new ConnectionSettings(new Uri(uri))
                .DefaultIndex(IndexName);

            _client = new ElasticClient(settings);
        }

        public async Task IndexComment(CommentIndex comment)
        {
            await _client.IndexDocumentAsync(comment);
        }

        public async Task<List<CommentSearchResult>> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<CommentSearchResult>();

            query = query.Trim();

            var res = await _client.SearchAsync<CommentIndex>(s => s
                .Index(IndexName)
                .Size(20)
                .Query(q => q.Bool(b => b
                    .Should(
                        sh => sh.MultiMatch(m => m
                            .Fields(f => f
                                .Field(x => x.Text, 2.0)
                                .Field(x => x.UserName)
                            )
                            .Query(query)
                            .Fuzziness(Fuzziness.Auto)
                        ),
                        sh => sh.MatchPhrasePrefix(m => m
                            .Field(x => x.Text)
                            .Query(query)
                            .Boost(3)
                        ),
                        sh => sh.MatchPhrasePrefix(m => m
                            .Field(x => x.UserName)
                            .Query(query)
                        )
                    )
                    .MinimumShouldMatch(1)
                ))
                .Sort(st => st
                    .Descending(SortSpecialField.Score)
                    .Descending(f => f.CreatedAt)
                )
                .Highlight(h => h
                    .PreTags("<mark>")
                    .PostTags("</mark>")
                    .Fields(f => f
                        .Field(x => x.Text)
                        .NumberOfFragments(1)
                    )
                    .Fields(f => f
                        .Field(x => x.UserName)
                    )
                )
            );

            return res.Hits.Select(hit => new CommentSearchResult
            {
                Id = hit.Source.Id,
                UserName = hit.Source.UserName,
                Text = hit.Highlight != null && hit.Highlight.ContainsKey("text")
                    ? string.Join(" ", hit.Highlight["text"])
                    : hit.Source.Text
            }).ToList();
        }

        public async Task InitAsync()
        {
            var exists = await _client.Indices.ExistsAsync(IndexName);
            if (exists.Exists)
                return;

            await _client.Indices.CreateAsync(IndexName, c => c
                .Settings(s => s
                    .Analysis(a => a
                        .Tokenizers(t => t
                            .EdgeNGram("edge_ngram_tokenizer", e => e
                                .MinGram(2)
                                .MaxGram(10)
                                .TokenChars(TokenChar.Letter, TokenChar.Digit)
                            )
                        )
                        .Analyzers(an => an
                            .Custom("autocomplete", ca => ca
                                .Tokenizer("edge_ngram_tokenizer")
                                .Filters("lowercase")
                            )
                            .Custom("search_analyzer", sa => sa
                                .Tokenizer("standard")
                                .Filters("lowercase")
                            )
                        )
                    )
                )
                .Map<CommentIndex>(m => m
                    .Properties(p => p
                        .Text(t => t
                            .Name(n => n.Text)
                            .Analyzer("autocomplete")
                            .SearchAnalyzer("search_analyzer")
                        )
                        .Text(t => t
                            .Name(n => n.UserName)
                            .Analyzer("autocomplete")
                            .SearchAnalyzer("search_analyzer")
                        )
                        .Date(d => d
                            .Name(n => n.CreatedAt)
                        )
                    )
                )
            );
        }
    }
}