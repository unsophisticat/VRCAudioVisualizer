# VRC Simple Audio Visualizer

---

A prefab that displays a circular array of objects that react to an audio source.

### Features:

- Fast Fourier transform via Unity's Blackman-Harris windowing function.
- Weighted frequencies for a more balanced distrubution.
- Supports customizable amount of displayed objects.
- Supports adjustable rotation speed, radius, and intensity.

### How to use:

-   Requires VRChat SDK3 for Worlds, the compatible Unity version and UdonSharp.
1.  Attach an audio source to the Audio Visualizer Prefab's Audio Source field.
2.  Note: If the video player used has both video and stream audio sources, ensure the appropriate audio souce is used.
3.  Attach a Game Object to be displayed to the Audio Visualizer Prefab's Object To Spawn field.
4.  Note: A sample Game Object cube is provided.
