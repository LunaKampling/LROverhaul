.PHONY: build
build:
	cd src && dotnet build

.PHONY: run
run:
	cd src && dotnet run --framework net8.0

.PHONY: run-windows
run-windows:
	cd src && dotnet run --framework net8.0-windows

.PHONY: clean
clean:
	cd src && dotnet clean

.PHONY: format
format:
	cd src && dotnet format linerider.sln