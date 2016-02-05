# jsondiffpatch.net
<!--- badges -->
[![Build Status](https://secure.travis-ci.org/wbish/jsondiffpatch.net.svg)](http://travis-ci.org/wbish/jsondiffpatch.net)
[![Code Coverage](https://codeclimate.com/github/wbish/jsondiffpatch.net/badges/coverage.svg)](https://codeclimate.com/github/wbish/jsondiffpatch.net/coverage)

Json object diffs and patching ([jsondiffpatch](https://github.com/benjamine/jsondiffpatch) compatible)

## Usage

The full JSON patch document format is documented at https://github.com/benjamine/jsondiffpatch. 

Note: Efficient array diff is currently NYI.

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

## Installing
nuget install jsondiffpatchdotnet

## Attributions
* [jsondiffpatch](https://github.com/benjamine/jsondiffpatch)
* [google-diff-match-patch](https://code.google.com/archive/p/google-diff-match-patch/)
* [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/)
