# Kiota Abstractions Library for dotnet

[![Build, Test, CodeQl](https://github.com/microsoft/kiota-abstractions-dotnet/actions/workflows/build-and-test.yml/badge.svg?branch=main)](https://github.com/microsoft/kiota-abstractions-dotnet/actions/workflows/build-and-test.yml) [![NuGet Version](https://buildstats.info/nuget/Microsoft.Kiota.Abstractions?includePreReleases=true)](https://www.nuget.org/packages/Microsoft.Kiota.Abstractions/)

The Kiota abstractions Library for dotnet is the dotnet library defining the basic constructs Kiota projects need once an SDK has been generated from an OpenAPI definition.

A [Kiota](https://github.com/microsoft/kiota) generated project will need a reference to the abstraction package to build and run.

Read more about Kiota [here](https://github.com/microsoft/kiota/blob/main/README.md).

## Using the Abstractions Library

```shell
dotnet add package Microsoft.Kiota.Abstractions --prerelease
```

## Debugging

If you are using Visual Studio Code as your IDE, the **launch.json** file already contains the configuration to build and test the library. Otherwise, you can open the **Microsoft.Kiota.Abstractions.sln** with Visual Studio.

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit [https://cla.opensource.microsoft.com](https://cla.opensource.microsoft.com).

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft
trademarks or logos is subject to and must follow
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.