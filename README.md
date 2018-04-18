# Configure

A simple application designed to overwrite configuration values in targeted locations. This can be
useful when your team development practise leads to developers committing configuration values which
are specific to each individual.

## Publish

You can create a standalone EXE (windows x64) by executing the publish.bat, which will produce
build/configure.exe.

## Configuration

The application requires an associated configure.yaml file located next to the exe. If executed without
this file, a default one be created ready for you to edit. An example YAML file is shown below.

```yaml
---
dry-run: yes
pause: no
nodes:
  - match:
      - C:/wip/websites/**/web.config
      - C:/wip/nuget/**/app.config
      - C:/wip/nuget/**/*.exe.config
      - C:/wip/nuget/**/web.config
    actions:
      - path: //appSettings/add[@key='MyApplication.Homepage']/@value
        value: http://www.aj.co.uk
        action: create
      - path: //appSettings/add[@key='MyApplication.SqlDatabase']
        action: remove
      - path: //appSettings/add[@key='MyApplication.MongoDB']/@value
        value: http://localhost:27017
      - appSetting: MyApplication.AdminUsername
        value: RichTea
...
```

### Data Model

#### dry-run (true/yes/false/no)

The dry-run property switches the application to not make any changes to files. The application runs
normally otherwise which helps debug match patterns and filters.

#### pause (true/yes/false/no/error)

This controls whether the application pauses on completion. This is not desirable when run from the
command line, but can be convenient when run as a GUI. The error value pauses only when an error occurs.

#### aliases (object)

The aliases object is where any anchors should be defined, if they are to be aliased elsewhere in the
document. In future versions this should not be constrained to this single property name, but due to
technical limitations at present it is.

#### nodes (list)

The nodes list is the meat of the configuration. It consists of one or more nodes, which at minimum
must have a list of match file pattern expressions, and a list of actions to execute on those files. It
is also possible to specify a name for better description and filtering.

## Console Arguments

The application can simply be run by executing the exe, which will run all the nodes in the associated
YAML configuration file. Optionally you can provide a single argument which specifies a subset of the
nodes to execute. For example:

`configure.exe Common,Website`

The names are comma separated, automatically trimmed for whitespace, and matched case sensitively. Any
nodes which don't have a name are treated as having a name of their 1-based index in the nodes array.
