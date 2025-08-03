#!/bin/sh
set -euxo pipefail

cd src && dotnet build
