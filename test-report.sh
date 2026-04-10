#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
RESULTS_DIR="${RESULTS_DIR:-${ROOT_DIR}/TestResults}"
TIMESTAMP="$(date +%Y%m%d-%H%M%S)"
TRX_FILE="test-results-${TIMESTAMP}.trx"
HTML_DIR="${HTML_DIR:-${RESULTS_DIR}/coverage-report-${TIMESTAMP}}"

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
echo "Restoring local .NET tools..."
dotnet tool restore

echo "Generating HTML coverage report..."
dotnet tool run reportgenerator \
  "-reports:${RESULTS_DIR}/**/coverage.cobertura.xml" \
  "-targetdir:${HTML_DIR}" \
  "-reporttypes:Html;HtmlSummary"

echo
echo "Test run complete."
echo "TRX report: ${RESULTS_DIR}/${TRX_FILE}"
echo "HTML coverage report: ${HTML_DIR}/index.html"
echo "Coverage files:"
find "${RESULTS_DIR}" -type f \( -name "*.xml" -o -name "*.cobertura.xml" -o -name "*.json" \) | sed 's/^/ - /'
