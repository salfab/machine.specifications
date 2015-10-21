using System;
using FluentAssertions;

namespace mspec_light.Tests
{
    [Subject(typeof(SampleSpecs), "Test Syntax")]
    public class When_run_a_Test_in_old_mspec_style : SampleSpecs
    {
        private static String myValue;

        private Establish context = () => myValue = ""; 

        private Because of = () => myValue = "Test";

        private It should_work_like_a_charm = () => myValue.Should().Be("Test");
    }

    public abstract class SampleSpecs : it
    {
    }
}
