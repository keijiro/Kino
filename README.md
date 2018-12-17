Kino
====

**Kino** is a collection of custom effects for Unity's [Post Processing Stack].

[Post Processing Stack]: https://github.com/Unity-Technologies/PostProcessing

System Requirements
-------------------

- Unity 2018.3 or later
- Post Processing Stack v2

Effects
-------

Currently Kino contains the following effects.

### Streak

![screenshot](https://i.imgur.com/FzwErHmm.jpg)

**Streak** adds horizontally stretched bloom that roughly resembles anamorphic
lens flares. Although it's neither physically correct nor energy conserving,
it's handy to emphasize shininess of the scene in just a few clicks.

### Recolor

![screenshot](https://i.imgur.com/uWiOrpDm.jpg)

**Recolor** is a kind of [false color] effect that replaces image colors by
mapping luminance to a given gradient. It also supports edge detection effect
to add contour lines to the images.

[false color]: https://en.wikipedia.org/wiki/False_color

### Overlay

**Overlay** simply adds a color gradient to the final output of the post
process. It's handy to widen the color spectrum of the output in a nearly
subliminal level.

### Isoline

![gif](https://i.imgur.com/yiiADOT.gif)

**Isoline** draws contour lines along a given axis. This is useful for creating
a "laser scan" effect.

How To Use
----------

### Trying out the examples

The example project contained in this repository uses [Git support on Package
Manager] that was newly added from Unity 2018.3. To enable this feature, Git
must be installed on the system. More specifically, you have to install [Git
for Windows] when using a Windows system, or Xcode for a Mac system.

[Git support on Package Manager]:
    https://forum.unity.com/threads/git-support-on-package-manager.573673/
[Git for Windows]: https://git-scm.com/downloads

### Using in your projects

Download and extract the [package zip file]. Move the extracted directory
(`Kino-upm`) into the `Packages` directory in your project.

You can also use [Git support on Package Manager] to import the package. Add
the following line to the `dependencies` section in the package manifest file
(`Packages/manifest.json`).

```
"jp.keijiro.kino.post-processing": "https://github.com/keijiro/jp.kino.post-processing.git#upm",
```

[package zip file]: https://github.com/keijiro/Kino/archive/upm.zip

License
-------

[MIT](Packages/jp.keijiro.kino.postprocessing/LICENSE.md)
