SOLUTION_DIR=$(PWD)
PROJECT=HelloLambda
OUTPUT_DIR=$(SOLUTION_DIR)/artifacts
ZIP=$(OUTPUT_DIR)/$(PROJECT).zip

.PHONY: restore build package clean

restore:
	dotnet restore $(PROJECT)/$(PROJECT).csproj

build:
	dotnet build $(PROJECT)/$(PROJECT).csproj -c Release

package: clean restore build
	mkdir -p $(OUTPUT_DIR)
	dotnet publish $(PROJECT)/$(PROJECT).csproj -c Release -o $(OUTPUT_DIR)/publish
	cd $(OUTPUT_DIR)/publish && zip -r $(ZIP) .

clean:
	rm -rf $(OUTPUT_DIR)


