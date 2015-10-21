# Mspec-Light

Mspec-light is a [context/specification][5] framework that removes language noise and simplifies tests. 
All it asks is that you accept the `= () =>`. Keep up with the [latest news and discussions][8] 
or follow the maintainers, [@agross](https://twitter.com/agross), 
CTO of [GROSSWEBER](http://grossweber.com/en),
[@danielmarbach](https://twitter.com/danielmarbach) and some more.

# Installation


# or:


# or:

The should library described above is an opinionated library provided by MSpec. You can also use other libraries like [FluentAssertions][12].

# Usage
MSpec is called a "context/specification" test framework because of the "grammar" that is used in describing and 
coding the tests or "specs". That grammar reads roughly like this

> When the system is in such a state, and a certain action occurs, it should do such-and-such or be in some end state.

You should be able to see the components of the traditional [Arrange-Act-Assert][9] model in there. 
To support readability and remove as much "noise" as possible, MSpec eschews the traditional attribute-on-method model 
of test construction. It instead uses custom .NET delegates that you assign anonymous methods and asks you to name them 
following a certain convention.

Read on to construct a simple MSpec styled specification class.

## Subject

The `Subject` attribute is the first part of a spec class. It describes the "context", which can be the 
literal `Type` under test or a broader description. The subject is not required, but it is good practice to add it. 
Also, the attribute allows [ReSharper](#resharper-integration) to detect context classes such 
that [delegate members](#establish) will not be regarded as unused.

The class naming convention is to use `Sentence_snake_case` and to start with the word "When".

```csharp
[Subject("Authentication")]                           // a description
[Subject(typeof(SecurityService))]                    // the type under test
[Subject(typeof(SecurityService), "Authentication")]  // or a combo!
public class When_authenticating_a_user { ... }       // remember: you can only use one Subject Attribute!
```

## Tags

The `Tags` attribute is used to organize your spec classes for inclusion or exclusion in test runs. You can identify tests that hit the database by tagging them "Slow" or tests for special reports by tagging them "AcceptanceTest".

Tags can be used to [include or exclude certain contexts during a spec run](#command-line-reference).

```csharp
[Tags("RegressionTest")]  // this attribute supports any number of tags via a params string[] argument!
[Subject(typeof(SecurityService), "Authentication")]
public class When_authenticating_a_user { ... }
```

## Establish

The `Establish` delegate is the "Arrange" part of the spec class. The `Establish` will only run *once*, so your assertions should not mutate any state or you may be in trouble.

```csharp
[Subject("Authentication")]
public class When_authenticating_a_new_user
{
    Establish context = () =>
    {
        // ... any mocking, stubbing, or other setup ...
        Subject = new SecurityService(foo, bar);
    };

    static SecurityService Subject;
}
```

## Cleanup

The pair to Establish is `Cleanup`, which is also called *once* after all of the specs have been run.

```csharp
[Subject("Authentication")]
public class When_authenticating_a_user
{
    Establish context = () =>
    {
        Subject = new SecurityService(foo, bar);
    };

    Cleanup after = () =>
    {
        Subject.Dispose();
    };

    static SecurityService Subject;
}
```

## Because

The `Because` delegate is the "Act" part of the spec class. It should be the single action for this context, the only part that mutates state, against which all of the assertions can be made. Most `Because` statements are only *one* line, which allows you to leave off the squiggly brackets!

```csharp
[Subject("Authentication")]
public class When_authenticating_a_user
{
    Establish context = () =>
    {
        Subject = new SecurityService(foo, bar);
    };

    Because of = () => Subject.Authenticate("username", "password");

    static SecurityService Subject;
}
```

If you have a multi-line `Because` statement, you probably need to identify which of those lines are actually setup and move them into the `Establish`. Or, your spec may be concerned with too many contexts and needs to be split or the subject-under-test needs to be refactored.

## It

The `It` delegate is the "Assert" part of the spec class. It may appear one or more times in your spec class. Each statement should contain a single assertion, so that the intent and failure reporting is crystal clear. Like `Because` statements, `It` statements are usually one-liners and may not have squiggly brackets.

```csharp
[Subject("Authentication")]
public class When_authenticating_an_admin_user
{
    Establish context = () =>
    {
        Subject = new SecurityService(foo, bar);
    };

    Because of = () => Token = Subject.Authenticate("username", "password");

    It should_indicate_the_users_role = () => Token.Role.ShouldEqual(Roles.Admin);
    It should_have_a_unique_session_id = () => Token.SessionId.ShouldNotBeNull();

    static SecurityService Subject;
    static UserToken Token;
}
```

An `It` statement without an assignment will be reported by the test runner in the "Not implemented" state. You may find that "stubbing" your assertions like this helps you practice TDD.

```csharp
It should_list_your_authorized_actions;
```

### Assertion Extension Methods

As you can see above, the `It` assertions make use of these (ShouldEqual, ShouldNotBeNull)  `Should` extension methods. They encourage readability and a good flow to your assertions when read aloud or on paper. You *should* use them wherever possible, just "dot" off of your object and browse the IntelliSense!

It's good practice to make your own `Should` assertion extension methods for complicated custom objects or domain concepts.

## Catch

When testing that exceptions are thrown from the "action" you should use a `Catch` statement. This prevents thrown exceptions from escaping the spec and failing the test run. You can inspect the exception's expected properties in your assertions.

```csharp
[Subject("Authentication")]
public class When_authenticating_a_user_fails_due_to_bad_credentials
{
    Establish context = () =>
    {
        Subject = new SecurityService(foo, bar);
    };

    Because of = () => Exception = Catch.Exception(() => Subject.Authenticate("username", "password"));

    It should_fail = () => Exception.ShouldBeOfType<AuthenticationFailedException>();
    It should_have_a_specific_reason = () => Exception.Message.ShouldContain("credentials");

    static SecurityService Subject;
    static Exception Exception;
}
```

# Command Line Reference

### TeamCity Reports

MSpec can output [TeamCity](http://www.jetbrains.com/teamcity/) [service messages][7] to update the test run status in real time. This feature is enabled by passing the `--teamcity` switch, but the command-line runner *can* auto-detect that it is running in the TeamCity context.

More information can be found under [the reporting repo](https://github.com/machine/machine.specifications.reporting). Please provide feedback, feature requests, issues and more in that repository.

### HTML Reports

MSpec can output human-readable HTML reports of the test run by passing the `--html` option. If a filename is provided, the output is placed at that path, overwriting existing files. If multiple assemblies are being testing, the output is grouped into a single file. If no filename is provided, it will use the name of the assembly(s). If multiple assemblies are being tested, an `index.html` is created with links to each assembly-specific report. You can use this option if your CI server supports capturing HTML as build reports.

More information can be found under [the reporting repo](https://github.com/machine/machine.specifications.reporting). Please provide feedback, feature requests, issues and more in that repository.

### XML Reports

MSpec can output XML test run reports by passing the `--xml` option. This option behaves the same as the `--html` option, in terms of file naming.

More information can be found under [the reporting repo](https://github.com/machine/machine.specifications.reporting). Please provide feedback, feature requests, issues and more in that repository.


More information can be found under [the TDnet repo](https://github.com/machine/machine.specifications.runner.tdnet). Please provide feedback, feature requests, issues and more in that repository.

 [3]: https://teamcity.bbv.ch/project.html?projectId=MachineSpecifications
 [4]: http://nuget.org/packages/Machine.Specifications
 [5]: http://www.code-magazine.com/article.aspx?quickid=0805061
 [6]: http://codebetter.com/blogs/aaron.jensen/archive/2009/10/19/advanced-selenium-logging-with-mspec.aspx
 [7]: http://confluence.jetbrains.com/display/TCD9/Build+Script+Interaction+with+TeamCity#BuildScriptInteractionwithTeamCity-ReportingTests
 [8]: https://groups.google.com/forum/?fromgroups#!forum/machine_users
 [9]: http://c2.com/cgi/wiki?ArrangeActAssert
 [10]: https://github.com/agross/mspec-samples/tree/master/WebSpecs/LoginApp.Selenium.Specs
 [11]: http://therightstuff.de/2010/03/03/MachineSpecifications-Templates-For-ReSharper.aspx
 [12]: https://github.com/dennisdoomen/fluentassertions
