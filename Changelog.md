# Changelog for Serilog.Enrichers.Sensitive

## 1.7.4

- Add masking options for properties [#29](https://github.com/serilog-contrib/Serilog.Enrichers.Sensitive/issues/29)

## 1.7.3

- Add support for masking property values that contain a `Uri`, reported by @yadanilov19

## 1.7.2

- FIx issue where Microsoft.Extensions.Configuration.Binder version 7.0.0 or up was required for JSON configuration to work. [#25](https://github.com/serilog-contrib/Serilog.Enrichers.Sensitive/issues/25)

## 1.7.1

- Support confguring masking operators using [Serilog.Settings.Configuration](https://github.com/serilog/serilog-settings-configuration) (mainly `appsettings.json`)

## 1.7.0

- Allow configuration through `appsettings.json` files.

## 1.6.0

- Pass match to `PreprocessMask` to allow for further customisation of mask value

## 1.5.1 

- Add support for dictionaries in destructured objects

## 1.5.0

- Add support for collections in destructured objects
- Add support for destructured objects which are collections

## 1.4.0

- Add support for masking destructured objects
- Marked the `WithSensitiveDataMasking()` methods that take specific parameters as obsolete in favour of the `WithSensitiveDataMasking(options => {})` version

## 1.3.0

- Add the default enricher logo to package [#5](https://github.com/serilog-contrib/Serilog.Enrichers.Sensitive/issues/5)
- Add option to exclude properties from masking [#9](https://github.com/serilog-contrib/Serilog.Enrichers.Sensitive/issues/9)

## 1.2.0

- Add `ShouldMaskMatch` to the `RegexMaskingOperator` to allow implementors to perform further checks on the sensitive value in order to decide whether or not to perform masking

## 1.1.0

- Add support to supply a custom mask value [#6](https://github.com/serilog-contrib/Serilog.Enrichers.Sensitive/issues/6)
- Add support to always mask a property regardless of value [#7](https://github.com/serilog-contrib/Serilog.Enrichers.Sensitive/issues/7)

## 1.0.0

- Removed a number of try/catch blocks that only existed for debugging
- Updated copyright notices
- Bumped version
- Cleaned up README