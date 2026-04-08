#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
RESULTS_DIR="${RESULTS_DIR:-${ROOT_DIR}/TestResults}"
TIMESTAMP="$(date +%Y%m%d-%H%M%S)"
TRX_FILE="test-results-${TIMESTAMP}.trx"

mkdir -p "${RESULTS_DIR}"

echo "Running tests..."
echo "Results directory: ${RESULTS_DIR}"

dotnet test "${ROOT_DIR}/IssueSense.slnx" \
  --results-directory "${RESULTS_DIR}" \
  --logger "trx;LogFileName=${TRX_FILE}" \
  --collect:"XPlat Code Coverage" \
  -p:UseSharedCompilation=false \
  -maxcpucount:1 \
  -nr:false

echo
echo "Test run complete."
echo "TRX report: ${RESULTS_DIR}/${TRX_FILE}"
echo "Coverage files:"
find "${RESULTS_DIR}" -type f \( -name "*.xml" -o -name "*.cobertura.xml" -o -name "*.json" \) | sed 's/^/ - /'
