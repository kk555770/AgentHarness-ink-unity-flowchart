// ===== 變更開始 =====
// 2026/03/25 Opsidanos (修改原因：把預設 Ink compile probe 放進 canonical core，避免 Ink 驗證只能靠 Editor bootstrap 才能啟動)
// 預期結果：只要 `OpsidanosInk.CanonicalGraph` 可用，`ValidateProjection(Ink)` / `ProjectGraph(Ink)` 就有可用的預設編譯器 probe
using System;
using System.Collections.Generic;

namespace OpsidanosInk.CanonicalGraph
{
    public sealed class CurrentFlowDefaultInkCompileProbe : ICurrentFlowInkCompileProbe
    {
        public bool TryCompile(string inkContent, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                var compiler = new Ink.Compiler(
                    inkContent ?? string.Empty,
                    new Ink.Compiler.Options
                    {
                        errorHandler = (message, type) =>
                        {
                            if (type == Ink.ErrorType.Error)
                            {
                                errors.Add(message);
                            }
                        }
                    });

                Ink.Runtime.Story story = compiler.Compile();
                if (errors.Count > 0)
                {
                    errorMessage = string.Join(" | ", errors);
                    return false;
                }

                if (story == null)
                {
                    errorMessage = "Ink.Compiler.Compile() 回傳 null。";
                    return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                errorMessage = $"Ink 編譯器丟出例外：{exception.Message}";
                return false;
            }
            finally
            {
                errors.Clear();
            }
        }

        private readonly List<string> errors = new List<string>();
    }
}
// ===== 變更結束 =====
