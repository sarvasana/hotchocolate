using CookieCrumble;
using HotChocolate.Execution;

namespace HotChocolate.Data.Filters;

public class QueryableFilterVisitorBooleanTests : IClassFixture<SchemaCache>
{
    private static readonly Foo[] _fooEntities =
    {
        new() { Bar = true },
        new() { Bar = false }
    };

    private static readonly FooNullable[] _fooNullableEntities =
    {
        new()
        {
            Bar = true
        },
        new()
        {
            Bar = null
        },
        new()
        {
            Bar = false
        }
    };

    private readonly SchemaCache _cache;

    public QueryableFilterVisitorBooleanTests(SchemaCache cache)
    {
        _cache = cache;
    }

    [Fact]
    public async Task Create_BooleanEqual_Expression()
    {
        // arrange
        var tester = _cache.CreateSchema<Foo, FooFilterInput>(_fooEntities);

        // act
        var res1 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery("{ root(where: { bar: { eq: true}}){ bar}}")
                .Create());

        var res2 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery("{ root(where: { bar: { eq: false}}){ bar}}")
                .Create());

        // assert
        await Snapshot
            .Create().AddResult(
                res1,
                "true").AddResult(
                res2,
                "false")
            .MatchAsync();
    }

    [Fact]
    public async Task Create_BooleanNotEqual_Expression()
    {
        // arrange
        var tester = _cache.CreateSchema<Foo, FooFilterInput>(_fooEntities);

        // act
        var res1 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery("{ root(where: { bar: { neq: true}}){ bar}}")
                .Create());

        var res2 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery("{ root(where: { bar: { neq: false}}){ bar}}")
                .Create());

        // assert
        await Snapshot
            .Create().AddResult(
                res1,
                "true").AddResult(
                res2,
                "false")
            .MatchAsync();
    }

    [Fact]
    public async Task Create_NullableBooleanEqual_Expression()
    {
        // arrange
        var tester = _cache.CreateSchema<FooNullable, FooNullableFilterInput>(_fooNullableEntities);

        // act
        var res1 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery("{ root(where: { bar: { eq: true}}){ bar}}")
                .Create());

        var res2 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery("{ root(where: { bar: { eq: false}}){ bar}}")
                .Create());

        var res3 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery("{ root(where: { bar: { eq: null}}){ bar}}")
                .Create());

        // assert
        await Snapshot
            .Create().AddResult(
                res1,
                "true").AddResult(
                res2,
                "false").AddResult(
                res3,
                "null")
            .MatchAsync();
    }

    [Fact]
    public async Task Create_NullableBooleanNotEqual_Expression()
    {
        // arrange
        var tester = _cache.CreateSchema<FooNullable, FooNullableFilterInput>(
            _fooNullableEntities);

        // act
        var res1 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery("{ root(where: { bar: { neq: true}}){ bar}}")
                .Create());

        var res2 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery("{ root(where: { bar: { neq: false}}){ bar}}")
                .Create());

        var res3 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery("{ root(where: { bar: { neq: null}}){ bar}}")
                .Create());

        // assert
        await Snapshot
            .Create().AddResult(
                res1,
                "true").AddResult(
                res2,
                "false").AddResult(
                res3,
                "null")
            .MatchAsync();
    }

    public class Foo
    {
        public string? Id { get; set; }

        public bool Bar { get; set; }
    }

    public class FooNullable
    {
        public string? Id { get; set; }

        public bool? Bar { get; set; }
    }

    public class FooFilterInput : FilterInputType<Foo>
    {
    }

    public class FooNullableFilterInput : FilterInputType<FooNullable>
    {
    }
}
