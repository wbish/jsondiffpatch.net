# jsondiffpatch.net
<!--- badges -->
[![Build status](https://ci.appveyor.com/api/projects/status/aavhn0lwas0j29gy?svg=true)](https://ci.appveyor.com/project/wbish/jsondiffpatch-net)
[![NuGet](https://img.shields.io/nuget/v/JsonDiffPatch.Net.svg)](https://www.nuget.org/packages/JsonDiffPatch.Net/)

JSON object diffs and reversible patching ([jsondiffpatch](https://github.com/benjamine/jsondiffpatch) compatible)

## Installing

Install from [jsondiffpatch.net](https://www.nuget.org/packages/JsonDiffPatch.Net/) nuget website, or run the following command:

``` PowerShell
Install-Package JsonDiffPatch.Net
````

## Usage

The library has support for the following 3 operations: Diff, Patch and Unpatch.

### Diff

Diff two json objects

``` C#
  var jdp = new JsonDiffPatch();
  var left = JToken.Parse(@"{ ""key"": false }");
  var right = JToken.Parse(@"{ ""key"": true }");

  JToken patch = jdp.Diff(left, right);

  Console.WriteLine(patch.ToString());

  // Output:
  // {
  //     "key": [false, true]
  // }
```

### Patch

Patch a left object with a patch document

``` C#
  var jdp = new JsonDiffPatch();
  var left = JToken.Parse(@"{ ""key"": false }");
  var right = JToken.Parse(@"{ ""key"": true }");
  JToken patch = jdp.Diff(left, right);

  var output = jdp.Patch(left, patch);

  Console.WriteLine(output.ToString());

  // Output:
  // {
  //     "key": true
  // }
```

### Unpatch

Unpatch a right object with a patch document

``` C#
  var jdp = new JsonDiffPatch();
  var left = JToken.Parse(@"{ ""key"": false }");
  var right = JToken.Parse(@"{ ""key"": true }");
  JToken patch = jdp.Diff(left, right);

  var output = jdp.Unpatch(right, patch);

  Console.WriteLine(output.ToString());

  // Output:
  // {
  //     "key": false
  // }
```

## Advanced Usage

JsonDiffPatch.Net is designed to handle complex diffs by producing a compact diff object with enough information to patch and unpatch relevant JSON objects. The following are some of the most common cases you may hit when generating a diff:

- Adding, Removing a property from an object
- Changing the property value or even value type
- Inserting and shifting elements in an array
- Efficient string diffing using google-diff-match-patch
- Nested object diffs

The full JSON patch document format is documented at https://github.com/benjamine/jsondiffpatch.

``` JavaScript
var left =
{
  "id": 100,
  "revision": 5,
  "items": [
    "car",
    "bus"
  ],
  "tagline": "I can't do it. This text is too long for me to handle! Please help me JsonDiffPatch!",
  "author": "wbish"
}

var right =
{
  "id": 100,
  "revision": 6,
  "items": [
    "bike",
    "bus",
    "car"
  ],
  "tagline": "I can do it. This text is not too long. Thanks JsonDiffPatch!",
  "author": {
    "first": "w",
    "last": "bish"
  }
}

var jdp = new JsonDiffPatch();
var output = jdp.Diff(left, right);

// Output:
{
  "revision": [   // Changed the value of a property
    5,            // Old value
    6             // New value
  ],
  "items": {      // Inserted and moved items in an array
    "0": [
      "bike"
    ],
    "_t": "a",
    "_1": [
      "",
      1,
      3
    ]
  },
  "tagline": [    // A long string diff using google-diff-match-patch
    "@@ -2,10 +2,8 @@\n  can\n-'t\n  do \n@@ -23,49 +23,28 @@\n  is \n+not \n too long\n- for me to handle! Please help me\n+. Thanks\n  Jso\n",
    0,
    2
  ],
  "author": [     // Changed the type of the author property from string to object
    "wbish",
    {
      "first": "w",
      "last": "bish"
    }
  ]
}
```

## JSON Patches (RFC 6902)

A diff result can be converted into JSON patches, according to the [RFC 6902 spec](https://tools.ietf.org/html/rfc6902).

```csharp
var left = JObject.Parse("{ \"name\": \"Justin\" }");
var right = JObject.Parse("{ \"name\" : \"John\", \"age\": 34 }");
var patch = new JsonDiffPatch().Diff(left, right);
var formatter = new JsonDeltaFormatter();
var operations = formatter.Format(patch);

/*
operations: [
  { "op": "replace", "path": "/name", "value": "John" },
  { "op": "add", "path": "/age", "value": 34 }
]
*/
```

## Attributions
* [jsondiffpatch](https://github.com/benjamine/jsondiffpatch)
* [google-diff-match-patch](https://github.com/google/diff-match-patch)
* [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/)
