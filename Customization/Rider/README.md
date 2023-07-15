# User folder
Custom files can be placed in user directory which can be opened by clicking the `Menu -> Open User Directory` button or navigating the `Documents/LRA/` directory manually.

# Custom scarves

## How to install
* Open the `Scarves` folder in the user directory.
* Paste text or image scarf file from [/Examples/Scarves/](/Examples/Scarves/) or any other source.
  * Or create a new text file, open it, and paste examples directly from this page.
* You can apply new scarf in `Preferences -> Application -> Rider -> Selected Scarf`.

## How to edit
A scarf file is an either text or image file. Both types support opacity.
### .png file
Scarf is an 1px tall image, each pixel represents separate scarf segments. For convenience, the first segment color is the most right pixel, just like in-game scarf's first segment is the most right one.

### .txt file
Scarf file has the following structure:
```
#Line Rider Scarf File
[Hex color], [Opacity]
[Hex color], [Opacity]
[Hex color], [Opacity]
...
```
Hex color is a `RRGGBB` value. Opacity is a value from `0%` (totally transparent) to `100%` (totally opaque). If transparency is not needed, the opacity value can be omitted.

### Notes

* If rider requires more colors than scarf file provides, colors repeat from the beginning.
* Default rider has 5 segments on his body and 5 segments flapping in the wind (the latter can be changed in `Preferences -> Application -> Rider -> Scarf segments`), resulting in 10 colors in total. Note that the first flapping segment (the 6th one for the default rider) is is always hidden behind the body, so it isn't visible.
* Body scarf segments are not affected by transparency.
* In game, the flapping part of the scarf can have only odd amount of segments (3, 5, 7, etc.).

## Examples
_Default.txt_
```
#Line Rider Scarf File
B93332
EF7A5D
```
_Rainbow.txt_
```
#Line Rider Scarf File
E40303
FF8C00
FFED00
008026
004DFF
750787
```

# Custom riders

_Note:_ custom riders affect neither game physics nor rider colliders.

## How to install
* Open the `Riders` folder in the user directory and add a folder that's the name of the rider.
* In the new folder paste/edit the files from [/Examples/Rider/Template/](/Examples/Rider/Template/) to create a new rider.
* You can then select the rider in the `Preferences -> Application -> Rider -> Selected Rider`.

_Note:_ default rider and its source files can be found in [/src/Resources/rider/](/src/Resources/rider/)

## How to edit

### **Rider**
_Files:_ `body.png`, `bodydead.png`, `sled.png`, `sledbroken.png`, `arm.png`, `leg.png`

Any file listed above has customizable resolution. Contact point positions always stick to the center of the image, check the template files to see their exact position.

If you want to add empty space around a body part, simply add the same amount of pixels to opposite sides keeping image centered. E.g. when increasing resolution from `1000x500` to `1200x600`, add **100** pixels to both left and right sides and **50** pixels to both the top and the bottom of the picture.

### **Scarf**
_Files:_ `regions.png`, `palette.png`, `.regions`

Rider can have dynamically changing colors which are taken from user selected scarf.

To add those, draw random colored rectangles over `body.png` and `bodydead.png` images (each rectangle should have an unique color), then create an 1px tall `regions.png` file with those exact colors in desired order. Regions image's width represents amount of colors to change. Take a look at examples to have a better understanding of how it works.

For faster rendering, regions get cached. It may take a while to generate a `.regions` file. Cache get auto-regenerated every time one of `body.png`, `bodydead.png`, or `regions.png` files is changed, so you don't have to handle cache manually.

`palette.png` is a legacy way to recolor rider's body. It works exactly the same way, the only difference is that it works with individual pixels instead of rectangles, resulting in slower skin initialization but giving an ability to recolor complex shapes. Note that palette file is not cached as it's here mostly for compatibility reasons.

### **Rope**
_Files:_ `rope.png`

The sled rope color and width can also be tweaked. Image width represents rope thickness and its first pixel is the rope color/opacity.

If file is missing, render fallbacks to default value that's a black 14px rope (for legacy skins, the value is ≈7px).

# Legacy skins
The game supports scarves and skins made for older LR versions (such as LRTran or LRA:CE).

## Scarves
Legacy loader is enabled if a scarf file starts with a "#LRTran Scarf File" line. The rest of lines contain full hex values. Example: `0xfd4f38, 0xff`

## Riders
 It automatically switches to the old render handler if skin folder contains a `brokensled.png` file (its current name is `sledbroken.png` to harmonize sled/sledbroken with body/bodydead files). It checks file name as it's faster than validating images resolution.

Every image of legacy skin has different scale factor (varies from ≈0.013 to ≈0.017 to fit differently sized body parts into 1024x512 images), which means 5px wide line on body and 5px wide line on leg won't look the same in the game. The new image handler resolves the problem by introducing customizable resolution and an unified scale factor (0.015).