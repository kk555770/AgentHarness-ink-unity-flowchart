// ===== 變更開始 =====
// 2026/03/19 Opsidanos (修改原因：繼續薄化 Batch 3，將動作資料線 payload 從主節點檔抽成獨立型別檔)
// 預期結果：`InkFlowChartNodes.cs` 更聚焦主線節點本體；typed wire payload 則改由獨立檔維護
using System;

namespace OpsidanosInk.Editor
{
    // ===== 變更開始 =====
    // 2026/02/25 Opsidanos (修改原因：新增「資料連線」型別，讓動作節點與對話節點用型別線區分流程線)
    // 預期結果：GraphToolkit 會把資料連線視為 typed wire，與 Flow 線在視覺與連線規則上分離
    [Serializable]
    public struct InkFlowActionPayload
    {
    }
    // ===== 變更結束 =====
}
// ===== 變更結束 =====
