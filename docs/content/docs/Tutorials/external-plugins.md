---
title: External Plugins
category: tutorial
menu_order: 3
pre: "<i class='fas fa-external-link-alt'></i> "

---

# Using external plugins

To consume external plugins that aren't included in the `Myriad.Plugins` package, you must register them with Myriad. If you are using the CLI tool then the way to do this is by passing in the `--plugin <path to dll>` command-line argument. If you are using MSBuild then this can be done by adding to the `MyriadSdkGenerator` property to your project file:

```xml
<ItemGroup>
    <MyriadSdkGenerator Include="<path to plugin dll>" />
</ItemGroup>
```

For example, if you had a project layout like this:

```
\src
-\GeneratorLib
 - Generator.fs
 - Generator.fsproj
-\GeneratorTests
 - Tests.fs
 - GeneratorTests.fsproj
```

You would add the following to Generator.fsproj:
```xml
  <ItemGroup>
    <Content Include="build\Generator.props">
      <Pack>true</Pack>
      <PackagePath>%(Identity)</PackagePath>
      <Visible>true</Visible>
    </Content>
  </ItemGroup>
```

Then add a new folder `build` with the `Generator.props` file within:
```xml
<Project>
    <ItemGroup>
        <MyriadSdkGenerator Include="$(MSBuildThisFileDirectory)/../lib/netstandard2.1/Generator.dll" />
    </ItemGroup>
</Project>
```

Often an additional props file (In this smaple the file would be `Generator.InTest.props`) is used to make testing easier.  The matching element for the tests fsproj would be something like this:

```xml
<Project>
    <ItemGroup>
        <MyriadSdkGenerator Include="$(MSBuildThisFileDirectory)/../bin/$(Configuration)/netstandard2.1/Generator.dll" />
    </ItemGroup>
</Project>
```

Notice the Include path is pointing locally rather than within the packaged nuget folder structure.

In your testing `fsproj` you would add the following to allow the plugin to be used locally rather that having to consume a nuget package:

```xml
<!-- include plugin -->
<Import Project="<Path to Generator plugin location>\build\Myriad.Plugins.InTest.props" />
```

## Framework Compatibility

Myriad supports loading plugins built against a **different target framework** than the Myriad tool itself. For example, a plugin targeting `net8.0` can be loaded by the `net9.0` Myriad tool without errors.

The plugin loader uses `PreferSharedTypes = true`, which means assemblies already loaded by the Myriad host — such as `FSharp.Core` and `Fantomas.FCS` — are shared with the plugin rather than loaded again from the plugin's output directory. This prevents `System.Reflection.ReflectionTypeLoadException` that would otherwise occur due to type identity mismatches across `AssemblyLoadContext` boundaries.

> **Note:** While cross-framework plugin loading is supported, it is still recommended to target the same framework version as the Myriad tool for the most predictable behaviour.
