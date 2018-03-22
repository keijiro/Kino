Kino
====

**Kino** is a collection of custom effects for Unity's [Post Processing Stack].

[Post Processing Stack]: https://github.com/Unity-Technologies/PostProcessing

System Requirements
-------------------

- Unity 2018.1 or later
- Post Processing Stack v2

Effects
-------

Currently Kino contains the following three effects.

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

How To Use
----------

### Trying the demo project

If you know how to deal with [Git submodules], clone this repository and
do `subdmoule init` and `submodule update` in the repository. You can also
use the `--recursive` option to automatically init & update submodules when
cloning.

In case you don't prefer using Git submodules, download the project from the
[project zip file] link. Then download and extract the following packages into
the `Packages` directory in the project.

- [jp.keijiro.danish-statues](https://github.com/keijiro/DanishStatues/archive/upm.zip)
- [jp.keijiro.test-assets](https://github.com/keijiro/jp.keijiro.test-assets/archive/master.zip)

[Git submodules]: https://git-scm.com/book/en/v2/Git-Tools-Submodules
[project zip file]: https://github.com/keijiro/Kino/archive/master.zip

### Using in your projects

Download and extract the [package zip file]. Move the extracted directory
(`Kino-upm`) into the `Packages` directory in your project.

If you're using Git to manage your project, adding the `upm` branch of this
repository as a submodule in the `Packages` directory is also a handy way to
integrate it.

[package zip file]: https://github.com/keijiro/Kino/archive/upm.zip

License
-------

[MIT](Packages/jp.keijiro.kino.postprocessing/LICENSE.md)
