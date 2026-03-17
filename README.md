# ink-Unity integration

> 目前狀態更新：**這個 repo 已不只是上游 `ink-unity-integration` 套件鏡像**。  
> 到 **2026/03/17** 為止，這裡更接近一個「canonical 敘事圖規格 + current Unity Runtime + current Graph 作者工具工作流 + 驗證閉環」的工作倉庫。

## 先看這裡

如果你是第一次打開這個 repo，**不要只看下面保留的上游說明就開始判斷整個專案**。

目前更準確的理解方式是：

```text
Canonical narrative graph
  -> current GraphToolkit / .flowchart.json（目前作者投影與 round-trip 載體）
  -> .ink
  -> story.json
  -> Unity Runtime 播放
  -> Save / Load / Rollback / 測試驗證是否仍收斂
```

也就是說：

- `Documentation/` 裡的架構稿、schema、contract 與作者工具策略，才是理解 repo 北極星的正確入口
- `Packages/com.opsidanos.ink/` 是目前最成熟的 Unity Runtime / UI / Save / Presentation 套件
- `Assets/Editor/FlowChart/GraphToolkit/` 是**目前**仍可工作的作者工具主線，但不應被誤讀成未來唯一長期平台
- 本 README 後半保留了大量上游 `ink-unity-integration` 說明，**它仍有參考價值，但不等於這個 repo 的完整現況**

再講更白一點：

- **真相層**：`canonical schema / API / JSON contract`
- **現況作者工具層**：`GraphToolkit + .flowchart.json + .inkfc`
- **現況播放層**：`OpsidanosInk Runtime + story.json`
- **策略方向**：作者工具長期傾向 `Web-first`，而不是長期綁在 GraphToolkit 上
- **現況工作流文件**：`Documentation/CurrentAuthoringWorkflow.md`
- **重製邊界文件**：`Documentation/AuthoringRefactorBoundaries.md`
- **第一階段實作計畫**：`Documentation/AuthoringPhase1ImplementationPlan.md`
- **第一階段逐批次提案**：`Documentation/AuthoringPhase1ImplementationProposal.md`
- **Batch 0 開工清單**：`Documentation/AuthoringPhase1Batch0Kickoff.md`

若你想先看一張總地圖，請先讀：

- `Documentation/DocsIndex.md`

## 建議閱讀順序

### 1. 想先知道「整個 repo 的文件地圖」

先讀：

- `Documentation/DocsIndex.md`

這份文件會先告訴你：

- 哪些是北極星文件
- 哪些是現況工作流文件
- 哪些是 Runtime 使用入口
- 哪些是過渡層，不要誤讀成最終真相

### 2. 想先知道「這個專案到底在做什麼」

先讀：

- `Documentation/NarrativeGraphArchitecture.md`
- `Documentation/CanonicalGraphSchema.md`
- `Documentation/CanonicalGraphSchemaSpec.md`

這三份文件會先把：

- 真相在哪裡
- current GraphToolkit / `.flowchart.json` / `.ink` / `story.json` 各是什麼
- 哪些是 canonical core，哪些只是 projection / runtime adapter

講清楚。

### 3. 想知道「為什麼作者工具策略會往 Web-first 收斂」

先讀：

- `Documentation/AuthoringToolStrategy.md`

這份文件處理的是：

- 為什麼 AI 接入比較適合 web control surface
- GraphToolkit 在本 repo 的現況定位
- WebView / Browser / Electron 各自扮演什麼角色
- 為什麼現在不該把 GraphToolkit 當長期唯一平台

### 4. 想知道「之後重製時，應該先切哪一刀」

先讀：

- `Documentation/AuthoringRefactorBoundaries.md`

這份文件處理的是：

- 目前 GraphToolkit 腳本裡哪些責任混在一起
- 哪些該留在 current tooling shell
- 哪些該抽成 canonical core / projection adapter / future bridge

### 5. 想知道「第一階段真正要先改哪些檔」

先讀：

- `Documentation/AuthoringPhase1ImplementationPlan.md`

這份文件處理的是：

- 第一階段做什麼、不做什麼
- 先改哪些檔、先不動哪些檔
- 要跑哪些測試來守住 round-trip 與 Runtime 閉環

### 6. 想知道「第一階段要分幾批做」

先讀：

- `Documentation/AuthoringPhase1ImplementationProposal.md`

這份文件處理的是：

- 第一階段應拆成哪幾批
- 每一批先改哪些檔
- 每一批的 gate 測試是什麼
- 哪些情況下不該硬進下一批

### 7. 想知道「現在第一批就先做什麼」

先讀：

- `Documentation/AuthoringPhase1Batch0Kickoff.md`

這份文件處理的是：

- 第一批要新增哪些檔
- 哪個 asmdef 要先改
- 最小 core 測試先補哪幾支
- 什麼狀況下才算可以進 Batch 1

### 8. 想知道「目前作者工具輸出必須遵守什麼」

若你想先知道「這條 current GraphToolkit workflow 本身怎麼跑」，先讀：

- `Documentation/CurrentAuthoringWorkflow.md`

先讀：

- `Documentation/DeveloperModeOutputContract.md`

這份文件處理的是：

- current Flow Chart / GraphToolkit 工作流輸出 `.ink + .flowchart.json` 的合法契約
- `char` JSON、`transition.steps`
- Restore / Rollback / 可重播閉環

### 9. 想知道「AI / 程式之後會接什麼控制面」

先讀：

- `Documentation/CanonicalGraphApiSpec.md`
- `Documentation/CanonicalGraphJsonContract.md`

這兩份文件處理的是：

- AI / 程式應操作哪一層
- 為什麼控制面應建立在 canonical graph 上
- 為什麼 Plain JSON contract 是第一個正式控制面

### 10. 想知道「Unity 玩家模式怎麼接起來」

先讀：

- `Packages/com.opsidanos.ink/README.md`

這份文件比較像玩家模式入口，會告訴你：

- `InkStoryEngine`
- `VNPlayerPresenter`
- `InkSaveSystem`
- `InkTagEventRouter`
- `InkResourceMap`

怎麼在 Unity 場景裡接起來。

### 11. 想看目前 code 主線

常看的位置：

- `Packages/com.opsidanos.ink/Runtime/Scripts/Story/`
- `Packages/com.opsidanos.ink/Runtime/Scripts/Presentation/`
- `Packages/com.opsidanos.ink/Runtime/Scripts/Save/`
- `Packages/com.opsidanos.ink/Runtime/Scripts/UI/`
- `Assets/Editor/FlowChart/GraphToolkit/`
- `Assets/Editor/Tests/`
- `Assets/Tests/PlayMode/`

## 這個 repo 現在最重要的幾件事

- **Schema-first**：真正的語意真相不應該綁死在 GraphToolkit、Unity 或某一份 sidecar。
- **Projection-aware**：`.flowchart.json`、`.ink`、`story.json`、Unity 畫面都很重要，但它們是投影或 adapter，不是 canonical truth 本體。
- **Runtime 已經很成熟**：目前最穩的是 `StoryOutput`、Tag router、演出播放器、Save/Load/Rollback、UI 節奏控制。
- **GraphToolkit 已有正式主線**：`.inkfc <-> .flowchart.json + .ink` 的匯入、匯出、round-trip 都已經有測試護欄，但它目前更像 current tooling baseline，不是未來唯一長期平台。
- **Authoring strategy 已開始轉向**：長期作者工具策略傾向 `Web-first` control surface，讓 AI、瀏覽器自動化與前端演進不必綁死在 Unity Editor experimental 套件上。

## 與下方內容的關係

下面的 `Overview` 之後內容，主要還是保留上游 `ink-unity-integration` 的功能說明：

- Ink Player Window
- 自動編譯 `.ink -> .json`
- Inspector tools
- 上游安裝方式

這些內容依然有參考價值，尤其是理解 Ink Unity Integration 這層地基時。  
但如果你要理解**這個 repo 今天真正的產品方向**，請以前面的「先看這裡」與「建議閱讀順序」為主。

This Unity package allows you to integrate inkle's [ink narrative scripting language](http://www.inklestudios.com/ink) with Unity and provides tools to **compile**, **play** and **debug** your stories.

# Overview

 - **Using ink in your game**: Allows running and controlling ink files in Unity via the [C# runtime API](https://github.com/inkle/ink/blob/master/Documentation/RunningYourInk.md).
  
 - **ink player**: Provides a powerful [Ink Player Window](https://github.com/inkle/ink-unity-integration/blob/master/Documentation/InkPlayerWindow.md) for playing and debugging stories.
 
 - **Auto compilation**: Instantly creates and updates a JSON story file when a `.ink` is updated.
  
 - **Inspector tools**: Provides an icon for ink files, and a custom inspector that provides information about a file.

# Getting started

## :inbox_tray: Installation
There are 4 different ways to install this plugin:

### :star:As a .UnityPackage:star:
This will import the source into your Assets folder. This is a good option if you intend to edit the source for your own needs.
* [Download the latest .UnityPackage](https://github.com/inkle/ink-unity-integration/releases).
* Open the downloaded file to import it into your Unity project.

### As a UPM Package
Installing via a package allows you to easily update via Unity's Package Manager window. This is best if you don't need to edit the source.
* When installed via UPM, demo projects can be imported from Packages > Ink Unity Integration > Demos

#### Via Package Manager
* Add the following line to PROJECT ROOT/Packages/manifest.json:
`"com.inkle.ink-unity-integration": "https://github.com/inkle/ink-unity-integration.git#upm"`
#### OpenUPM
* Navigate to [OpenUPM](https://openupm.com/packages/com.inkle.ink-unity-integration/) and follow their instructions
* The project will have installed at Packages > Ink Unity Integration.


### From GitHub
* You can clone/download/fork the project on [GitHub](https://github.com/inkle/ink-unity-integration).
* The easiest way to download it is to click the green Code button and select Download ZIP
* Install by moving the folder Packages/Ink to anywhere in your Unity project's Assets folder

### Via the Asset Store

For convenience a .UnityPackage is hosted at the [Unity Asset Store](https://assetstore.unity.com/packages/tools/integration/ink-unity-integration-60055).
**This version is updated rarely, and so is not recommended.**
This will import the source into your Assets folder. This is a good option if you intend to edit the source for your own needs.



## :video_game: Demos
This project includes a demo scene, providing a simple example of how to control an ink story with C# code using Unity UI.

(If you imported this package as a UPM, then you must first import the demos from Packages > Ink Unity Integration > Demos)

To run a demo, double-click the scene file at the root of the demo folder to open it, and press the Play button at the top of the screen to start it.

## :page_facing_up: C# API
The C# API provides all you need to control ink stories in code; advancing your story, making choices, diverting to knots, saving and loading, and much more.

[It is documented in the main ink repo](https://github.com/inkle/ink/blob/master/Documentation/RunningYourInk.md#getting-started-with-the-runtime-api).

For convenience, the package also creates an (**Help > Ink > API Documentation**) menu option.

## :pencil2: Writing ink
For more information on writing with **ink**, see [the documentation in the main ink repo](https://github.com/inkle/ink). 

For convenience, the package also creates an (**Help > Ink > Writing Tutorial**) menu option.


## :question: Further Help
For assistance with writing or code, [Inkle's Discord forum](https://discord.gg/tD8Am2K) is full of lovely people who can help you out!

To keep up to date with the latest news about ink [sign up for the mailing list](http://www.inklestudios.com/ink#signup).


# Features

## Compilation
  
Ink files must be compiled to JSON before they can be used in-game. 
**This package compiles all edited ink files automatically.**
By default, compiled files are created next to their ink file.

### Editor Compilation
This package provides tools to automate this process when a .ink file is edited. 

**Disabling auto-compilation**: You might want to have manual control over ink compilation. If this is the case, you can disable "Compile ink automatically" in the InkSettings file or delete the InkPostProcessor class.

**Manual compilation**: If you have disabled auto-compilation, you can manually compile all ink files using the **Assets > Recompile Ink** menu item, individually via the inspector of an ink file, or via code using InkCompiler.CompileInk().

**Play mode delay**: By default, ink does not compile while in play mode. This can be disabled in the InkSettings file.

### In-game Compilation

The compiler is included in builds (See [WebGL best practices](#WebGLBestPractices) for information on removing it), enabling you to allow the editing of ink files as part of your game.


## <a name="InkPlayerWindow"></a>Ink Player Window

The Ink Player Window (**Window > Ink Player**) allows you to play stories in an editor window, and provides functionality to edit variables on the fly, test functions, profile performance, save and load states, and divert.

To play a story, click the "play" button shown on the inspector of a compiled ink file, or drag a compiled ink story TextAsset into the window.

**Editor Attaching**: Attaching the InkStory instance used by your game to the Ink Player window allows you to view and edit your story as it runs in game. 

See BasicInkExampleEditor.cs in the Examples folder for an example of how to:
* Show an attach/detach button on an inspector
* Automatically attach on entering play mode

[More information on using and extending Ink Player Window](https://github.com/inkle/ink-unity-integration/blob/master/Documentation/InkPlayerWindow.md)


## Inspector tools

This package replaces the icon for ink files to make them easier to spot, and adds a custom inspector for a selected ink file.

**The Inspector**: Selecting an ink file displays its last compile time; lists any include files; and shows any errors, warnings or todos. It also shows a Play button which runs the story in the Ink Player Window.


# Visual Scripting Support

## Bolt
There is currently no support for Bolt, Unity's official visual scripting tool. If you're interested in building one, we'd love to see it!

## PlayMaker
There's [unofficial support for PlayMaker here.](https://github.com/inkle/ink-unity-integration/issues/22) 


We'd love to see this supported more if you'd like to assist the effort!


# Source control tips

When you edit ink files, the compiler will also update the corresponding compiled .json file. If no compiled file existed before, Unity will also create a meta file for it. It is recommended that you always commit both ink and json files at the same time to avoid the file being re-compiled by your team members.

Adding or removing ink files will also make changes to the InkLibrary file, and we could recommend authors also commit this file for the same reasons.


# <a name="WebGLBestPractices"></a>WebGL best practices

WebGL builds should be as small as possible. The ink compiler is included in builds, but is typically only used in the editor. 
If your game doesn't require compiling ink at runtime we recommend adding a .asmdef at Ink Unity Integration > InkLibs > InkCompiler that only functions in the editor.


# FAQ

* Is the Linux Unity Editor supported?

  *Yes!*

* What versions of Unity are supported?

  We support 2020 LTS and above.
  Until version 1.1.1 we supported 2018 LTS, which should also work going back to at least Unity 5.

# Support us! :heart:

Ink is free, forever; but we'd really appreciate your support!
If you're able to give back, generous donations at our [Patreon](https://www.patreon.com/inkle) mean the world to us. 

# Discord:

Looking for help or want to meet likeminded writers/developers? Come say hello on our [Discord](https://discord.gg/inkle) server! 

# License

**ink** and this package is released under the MIT license. Although we don't require attribution, we'd love to know if you decide to use **ink** a project! Let us know on [Twitter](http://www.twitter.com/inkleStudios) or [by email](mailto:info@inklestudios.com).
View the full licence [Here](https://github.com/inkle/ink-unity-integration/blob/master/LICENCE.md)
