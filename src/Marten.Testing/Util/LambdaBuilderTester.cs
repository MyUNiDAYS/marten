﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Baseline.Reflection;
using Marten.Linq;
using Marten.Testing.Documents;
using Marten.Util;
using Shouldly;
using Xunit;

namespace Marten.Testing.Util
{
    public class LambdaBuilderTester
    {
        [Fact]
        public void can_build_getter_for_property()
        {
            var target = new Target {Number = 5};
            var prop = ReflectionHelper.GetProperty<Target>(x => x.Number);

            var getter = LambdaBuilder.GetProperty<Target, int>(prop);

            getter(target).ShouldBe(target.Number);
        }

        public class GuyWithField
        {
            public Guid Id = Guid.NewGuid();
        }

        [Fact]
        public void can_build_getter_for_field()
        {
            var guy = new GuyWithField();

            var field = typeof(GuyWithField).GetField("Id");

            var getter = LambdaBuilder.GetField<GuyWithField, Guid>(field);

            getter(guy).ShouldBe(guy.Id);
        }

        [Fact]
        public void can_build_setter_for_property()
        {
            var target = new Target { Number = 5 };
            var prop = ReflectionHelper.GetProperty<Target>(x => x.Number);

            var setter = LambdaBuilder.SetProperty<Target, int>(prop);

            setter(target, 11);


            target.Number.ShouldBe(11);
        }


        [Fact]
        public void can_build_setter_for_field()
        {
            var guy = new GuyWithField();

            var field = typeof(GuyWithField).GetField("Id");

            var setter = LambdaBuilder.SetField<GuyWithField, Guid>(field);


            var newGuid = Guid.NewGuid();

            setter(guy, newGuid);

            guy.Id.ShouldBe(newGuid);
        }

        [Fact]
        public void can_build_getter_for_deep_expression()
        {
            Expression<Func<Target, int>> expression = t => t.Inner.Number;

            var visitor = new FindMembers();
            visitor.Visit(expression);

            var members = visitor.Members.ToArray();

            var getter = LambdaBuilder.Getter<Target, int>(EnumStorage.AsInteger, members);

            var target = Target.Random(true);

            getter(target).ShouldBe(target.Inner.Number);
        }

        [Fact]
        public void can_get_the_Enum_GetName_method()
        {
            typeof(Enum).GetMethods().Count(x => x.Name == nameof(Enum.GetName) && !x.IsGenericMethod)
                .ShouldBe(1);
        }

        [Fact]
        public void can_set_a_private_id()
        {
            var member = ReflectionHelper.GetProperty<UserWithPrivateId>(x => x.Id);
            var setter = LambdaBuilder.Setter<UserWithPrivateId, Guid>(member);

            var newGuid = Guid.NewGuid();
            var userWithPrivateId = new UserWithPrivateId();

            setter(userWithPrivateId, newGuid);

            userWithPrivateId.Id.ShouldBe(newGuid);
        }
    }
}