Kino
====

**Kino** is a collection of custom post-processing effects for Unity's
[High Definition Render Pipeline][HDRP] (HDRP).

[HDRP]:
    https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@latest

System Requirements
-------------------

- Unity 2019.3
- HDRP 7.1

Effects
-------

### Streak

![screenshot](https://i.imgur.com/buCdMYm.gif)

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

### Glitch

![gif](https://i.imgur.com/bCLcgBi.gif)
![gif](https://i.imgur.com/kw85Pim.gif)

**Glitch** is a collection of simple video glitch effects.

- Block (block noise effect)
- Drift (color drift effect)
- Jitter (scan line jitter effect)
- Jump (vertical jump effect)
- Shake (horizontal shake effect)

### Sharpen

A simple sharpen filter that is similar to ones used in paint software.

### Utility

A multi-purpose filter that provides several small effects in a single pass.

- Hue shift
- Invert
- Fade (fade to color)

### Slice

![Slice](https://i.imgur.com/UdZvhqo.gif)

Slice and slide effect.

### Test Card

![Test Card](https://i.imgur.com/9kP6UFam.jpg)

A simple test card pattern generator.

How To Install
--------------

The Kino package uses the [scoped registry] feature to import dependent
packages. Please add the following sections to the package manifest file
(`Packages/manifest.json`).

To the `scopedRegistries` section:

```
{
  "name": "Keijiro",
  "url": "https://registry.npmjs.com",
  "scopes": [ "jp.keijiro" ]
}
```

To the `dependencies` section:

```
"jp.keijiro.kino.post-processing": "2.1.12"
```

After changes, the manifest file should look like below:

```
{
  "scopedRegistries": [
    {
      "name": "Keijiro",
      "url": "https://registry.npmjs.com",
      "scopes": [ "jp.keijiro" ]
    }
  ],
  "dependencies": {
    "jp.keijiro.kino.post-processing": "2.1.12",
    ...
```

[scoped registry]: https://docs.unity3d.com/Manual/upm-scoped.html

Frequently Asked Questions
--------------------------

#### Nothing happens when I add effects to a volume

Check the Default HDRP Settings in the Project Settings. You have to
define custom post processing oerders to make them take effect in the
pipeline.

![HDRP settings](https://i.imgur.com/v6Kddthl.jpg)

License
-------

[Unlicense](https://unlicense.org/)
