> [!CAUTION]
> This repo is a fork of [marten](https://github.com/JasperFx/marten), but has been reset to [v2.10.3](https://github.com/JasperFx/marten/releases/tag/2.10.3) (the last v2 release)

# Marten 
## Polyglot Persistence Powered by .NET and PostgreSQL

[![Join the chat at https://gitter.im/JasperFx/Marten](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/JasperFx/Marten?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![Windows Build Status](https://ci.appveyor.com/api/projects/status/github/jasperfx/marten?svg=true)](https://ci.appveyor.com/project/jasper-ci/marten)
[![Linux Build status](https://api.travis-ci.org/JasperFx/marten.svg)](https://travis-ci.org/JasperFx/marten)
[![Nuget Package](https://badgen.net/nuget/v/marten)](https://www.nuget.org/packages/Marten/)

![marten logo](http://jasperfx.github.io/marten/content/images/banner.png)


The Marten library provides .NET developers with the ability to use the proven [PostgreSQL database engine](http://www.postgresql.org/) and its [fantastic JSON support](https://www.compose.io/articles/is-postgresql-your-next-json-database/) as a fully fledged [document database](https://en.wikipedia.org/wiki/Document-oriented_database). The Marten team believes that a document database has far reaching benefits for developer productivity over relational databases with or without an ORM tool.

Marten also provides .NET developers with an ACID-compliant event store with user-defined projections against event streams.

> Note that current stable version of Marten 2.x is compatible with Npgsql 3.x. If you are looking for a Marten version compatible with Npgsql 4.x, you can use the Marten 3.x alpha version available in NuGet and also you can checkout the [dev branch](https://github.com/JasperFx/marten/tree/3.0).

## Working with the Code

Before getting started you will need the following in your environment:

* Install docker, if you don't already have it
* run `docker-compose up --remove-orphans -d` from the root of the repo

You are now ready to contribute to Marten.

### Tooling

* Unit Tests rely on [xUnit](http://xunit.github.io/) and [Shouldly](https://github.com/shouldly/shouldly)
* Rake is used for build automation. _It is not mandatory for development_.
* [Node.js](https://nodejs.org/en/) runs our Mocha specs.
* [Storyteller](http://storyteller.github.io) for some of the data intensive automated tests

### Mocha Specs

To run mocha tests use `rake mocha` or `npm run test`. There is also `npm run tdd` to run the mocha specifications
in a watched mode with growl turned on. 

> Note: remember to run `npm install`

### Storyteller Specs

To open the Storyteller editor, use the command `rake open_st` from the command line or `rake storyeller` to run the Storyteller specs. If you don't want to use rake, you can launch the
Storyteller editor *after compiling the solution* by the command `packages\storyteller\tools\st.exe open src/Marten.Testing`.

### Documentation

The documentation content is the markdown files in the `/documentation` directory directly under the project root. To run the documentation website locally with auto-refresh, either use the rake task `rake docs` or the batch script named `run-docs.cmd`. 

If you wish to insert code samples to a documentation page from the tests, wrap the code you wish to insert with
`// SAMPLE: name-of-sample` and `// ENDSAMPLE`.
Then to insert that code to the documentation, add `<[sample:name-of-sample]>`.

> Note: content is published to the `gh-pages` branch of this repository by running the `publish-docs.cmd` command.

### Rake Commands

```
# run restore, build and test
rake

# run all tests including mocha tests
rake test

# running documentation website locally
rake docs
```

### DotNet CLI Commands

```
# restore nuget libraries
dotnet restore src\Marten.sln

# build solution
dotnet build src\Marten.sln

# running tests for a specific target framework
dotnet test src\Marten.Testing\Marten.Testing.csproj

# mocha tests
npm install
npm run test

# running documentation website locally
dotnet restore docs.csproj
dotnet stdocs run
```
