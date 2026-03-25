#!/bin/zsh

# 這個腳本的目標很單純：
# - 在「終端機」一鍵跑完我們自己的測試（EditMode → PlayMode）
# - 不要誤跑到套件（Packages）裡的測試

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

UNITY_BIN_DEFAULT="/Applications/Unity/Hub/Editor/6000.3.9f1/Unity.app/Contents/MacOS/Unity"
UNITY_BIN="${UNITY_PATH:-${UNITY_BIN_DEFAULT}}"

RESULTS_DIR="${PROJECT_ROOT}/Logs/TestResults"
EVIDENCE_DIR="${PROJECT_ROOT}/Logs/UnityEvidence"
mkdir -p "${RESULTS_DIR}" "${EVIDENCE_DIR}"

if [[ ! -x "${UNITY_BIN}" ]]; then
  echo "[OpsidanosInk][錯誤] 找不到 Unity 執行檔：${UNITY_BIN}"
  echo "[OpsidanosInk][提示] 你可以先設定 UNITY_PATH，例如："
  echo "  UNITY_PATH=\"${UNITY_BIN_DEFAULT}\" \"${0}\""
  exit 1
fi

if [[ -f "${PROJECT_ROOT}/Temp/UnityLockfile" ]]; then
  echo "[OpsidanosInk][錯誤] 專案目前被 Unity 開著（Temp/UnityLockfile 存在）。"
  echo "[OpsidanosInk][提示] 請先關掉 Unity Editor，再跑一次這個腳本。"
  exit 1
fi

run_unity_tests() {
  local test_platform="$1"
  local assembly_name="$2"
  local result_xml_path="$3"
  local log_path="$4"

  echo "[OpsidanosInk] 開始跑 ${test_platform}（只跑：${assembly_name}）"
  echo "[OpsidanosInk] Unity log 會同步寫到：${log_path}"

  set +e
  "${UNITY_BIN}" \
    -batchmode \
    -nographics \
    -projectPath "${PROJECT_ROOT}" \
    -runTests \
    -testPlatform "${test_platform}" \
    -assemblyNames "${assembly_name}" \
    -testResults "${result_xml_path}" \
    -logFile - \
    > >(tee "${log_path}") 2>&1
  local rc=$?
  set -e

  if [[ "${rc}" -ne 0 ]]; then
    echo "[OpsidanosInk][錯誤] ${test_platform} 測試失敗（Unity 回傳碼：${rc}）"
    echo "[OpsidanosInk][錯誤] 失敗 log：${log_path}"
    return 1
  fi

  echo "[OpsidanosInk] ${test_platform} 測試通過"
  echo "[OpsidanosInk] ${test_platform} log：${log_path}"
  return 0
}

EDITMODE_XML="${RESULTS_DIR}/OpsidanosInk_EditMode.xml"
PLAYMODE_XML="${RESULTS_DIR}/OpsidanosInk_PlayMode.xml"
EDITMODE_LOG="${EVIDENCE_DIR}/OpsidanosInk_EditMode.log"
PLAYMODE_LOG="${EVIDENCE_DIR}/OpsidanosInk_PlayMode.log"

run_unity_tests "EditMode" "OpsidanosInk.EditModeTests" "${EDITMODE_XML}" "${EDITMODE_LOG}"
run_unity_tests "PlayMode" "OpsidanosInk.PlayModeTests" "${PLAYMODE_XML}" "${PLAYMODE_LOG}"

echo "[OpsidanosInk] 全部測試完成（EditMode → PlayMode）"
echo "[OpsidanosInk] 測試結果："
echo "  - ${EDITMODE_XML}"
echo "  - ${PLAYMODE_XML}"
echo "[OpsidanosInk] Evidence logs："
echo "  - ${EDITMODE_LOG}"
echo "  - ${PLAYMODE_LOG}"
