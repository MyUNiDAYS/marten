using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Threading;
using Marten.Linq.SoftDeletes;
using Marten.Schema;
using Marten.Services;
using Marten.Util;
using Npgsql;
using Xunit;

namespace Marten.Testing.Linq.SoftDeletes
{
    public class DeletedSinceParserTests
    {
        private readonly DocumentMapping _mapping;
        private readonly MethodCallExpression _expression;
        private readonly DeletedSinceParser _parser;

        public static TheoryData<CultureInfo> AllCultures
        {
            get
            {
                var data = new TheoryData<CultureInfo>();
                var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
                foreach (var culture in cultures)
                {
                    data.Add(culture);
                }
                return data;
            }
        }
        
        public DeletedSinceParserTests()
        {
            _mapping = new DocumentMapping(typeof(object), new StoreOptions()) {DeleteStyle = DeleteStyle.SoftDelete};
            _expression = Expression.Call(typeof(SoftDeletedExtensions).GetMethod(nameof(SoftDeletedExtensions.DeletedSince)),
                Expression.Parameter(typeof(object)),
                Expression.Constant(DateTimeOffset.UtcNow));
            _parser = new DeletedSinceParser();
        }

        [Theory]
        [MemberData(nameof(AllCultures))]
        public void WhereFragmentContainsExpectedExpression(CultureInfo cultureInfo)
        {
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
            
            var result = _parser.Parse(_mapping, new JsonNetSerializer(), _expression);
            
            var builder = new CommandBuilder(new NpgsqlCommand());
            
            result.Apply(builder);
            
            builder.ToString().ShouldContain("d.mt_deleted and d.mt_deleted_at >");
            builder.ToString().ShouldNotContain("?");
        }

        [Fact]
        public void ThrowsIfDocumentMappingNotSoftDeleted()
        {
            _mapping.DeleteStyle = DeleteStyle.Remove;

            Exception<NotSupportedException>.ShouldBeThrownBy(() => _parser.Parse(_mapping, new JsonNetSerializer(), _expression));
        }
    }
}
