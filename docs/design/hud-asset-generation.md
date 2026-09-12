# HUD素材の作成記録

組み込みimagegenを使用。画像案3を元にHPの枠と顔だけを生成した。
最初の出力には市松模様が描き込まれていたため、次の指示で単色のマゼンタ背景へ修正した。

最終プロンプト:

```text
Production pixel-art sprite cleanup. Preserve the existing LONG WHITE ANIMAL HEALTH FRAME and its right-hand face. REPLACE EVERY BIT OF THE GRAY CHECKERBOARD both outside the frame AND inside the hollow channel with perfectly flat, pure solid MAGENTA #FF00FF. There must be NO checkerboard, noise, texture, shadows or gradients in the magenta. Magenta is a temporary chroma key for the final transparent RGBA game asset. Keep only the cream-white frame and black outlining/facial pixels. Correct the long horizontal outline thickness to be uniformly 2 logical pixels cream with 1 pixel black. Use a hard low-resolution pixel grid as if native 256 x 40, output canvas preferably 256 x 40. Tight framing: silhouette almost fills width, aligned straight horizontally, no perspective; the face has two small square dot eyes and w mouth. The central channel MUST BE THE SAME SOLID #FF00FF as outside. Do not put any text or color fill in it. No other elements. Intended final postprocessed file /Users/yhei/unity/vlcnpStory2022/Assets/Game/UI/VeryLongHUD/very_long_health_frame_256x40.png, exact 256 x 40 pixels with keyed pixels alpha 0.
```

生成後にマゼンタを透過へ変換し、枠を白系と黒系の2色に整理した。
余白を除いて最近傍補間で256×36へ縮小し、上下2ピクセルの透過余白を加えて256×40とした。
完成PNGの寸法、外側と穴の透過、見た目を確認した。
UnityではPointフィルタ、圧縮なし、Mip Mapなしでインポートする。

枠の絵柄は[VeryLongAnimals公式素材](https://verylonganimals.com/download/)のガイネンアニマルを参考にしている。
