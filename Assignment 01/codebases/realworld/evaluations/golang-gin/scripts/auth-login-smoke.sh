#!/bin/bash

# Auth Login Smoke Test Runner
# This script runs the black-box tests for the auth-login-smoke feature

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
EVALUATIONS_DIR="$(dirname "$SCRIPT_DIR")"
TESTS_DIR="$EVALUATIONS_DIR/tests"

# Default configuration
DEFAULT_HOST="http://localhost:8080"
HOST="${HOST:-$DEFAULT_HOST}"
TIMEOUT="${TIMEOUT:-30s}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "========================================"
echo "Auth Login Smoke - Black-Box Tests"
echo "========================================"
echo ""
echo "Target Host: $HOST"
echo "Timeout: $TIMEOUT"
echo ""

# Check if host is reachable
echo -n "Checking if host is reachable... "
if curl -s --max-time 5 "$HOST" > /dev/null 2>&1 || curl -s --max-time 5 "$HOST/api" > /dev/null 2>&1; then
    echo -e "${GREEN}OK${NC}"
else
    echo -e "${YELLOW}WARNING${NC}: Host may not be reachable"
fi

# Run the Go tests
echo ""
echo "Running tests..."
echo ""

cd "$TESTS_DIR"

# Set environment variable for test configuration
export TEST_HOST="$HOST"
export TEST_TIMEOUT="$TIMEOUT"

# Run go test with verbose output
go test -v -timeout 60s ./...

TEST_EXIT_CODE=$?

echo ""
echo "========================================"
if [ $TEST_EXIT_CODE -eq 0 ]; then
    echo -e "${GREEN}All tests passed!${NC}"
else
    echo -e "${RED}Some tests failed. Exit code: $TEST_EXIT_CODE${NC}"
fi
echo "========================================"

exit $TEST_EXIT_CODE
