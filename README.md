# 🖌️ TCP Doodle Chat

**TCP Doodle Chat** 是一款基於 C# Windows Forms 和 TCP 通訊的多人協作聊天與繪圖工具。  
用戶可以即時透過網路連線，互相傳送公開或私密的文字訊息，並在同一畫布上協同繪圖。  
除了公開畫圖，還支援私密畫圖功能，允許對特定對象繪製只有他們可見的內容。

## ✨ 功能特色

### 🎨 繪圖功能
- 公開畫圖 (Pencil / Eraser)
- 私密畫圖 (Private Pencil / Private Eraser)，指定目標使用者
- 畫筆粗細與顏色設定
- 畫布清除 (Clear)
- 撤銷 (Undo) / 重做 (Redo)

### 💬 聊天功能
- 公開訊息
- 私密訊息 (指定對象)
- 使用者列表

### 📡 網路功能
- 透過 TCP 進行用戶連線與資料同步
- 斷線提示與自動回復功能
- 伺服器/用戶端架構

---

## 📦 專案架構

```

tcp-doodle-chat/
├── Form1.cs             // 主 UI，負責畫布、聊天、使用者列表等
├── DrawingManager.cs    // 繪圖邏輯 (畫圖、私密畫圖、Undo/Redo)
├── NetworkManager.cs    // TCP 連線與訊息傳送/接收
├── DrawAction.cs        // 訊息結構與序列化/反序列化邏輯
├── Program.cs           // 進入點
└── ...

```

---

## 🚀 快速開始

### 環境需求

- Windows 10+
- Visual Studio 2022+ (建議使用最新版本)
- .NET Framework 4.7.2+

### 建置與執行

1. Clone 專案
```bash
git clone https://github.com/viiccwen/tcp-doodle-chat.git
````

2. 使用 Visual Studio 開啟專案 (`.sln`)

3. 設定伺服器 IP 與 Port

4. 執行程式，選擇是否作為伺服器或用戶端，開始協作繪圖與聊天

---

## 📡 訊息協議 (DrawAction)

| Tool               | 功能描述      |
| ------------------ | --------- |
| ConnectServer      | 連線至伺服器    |
| DisconnectServer   | 斷線通知      |
| UpdateUserList     | 更新線上使用者列表 |
| SendMessage        | 公開訊息      |
| SendPrivateMessage | 私密訊息      |
| PublicPencil       | 公開畫圖      |
| PublicEraser       | 公開橡皮擦     |
| PrivatePencil      | 私密畫圖      |
| PrivateEraser      | 私密橡皮擦     |
| Clear              | 清除畫布      |
