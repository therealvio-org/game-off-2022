dist:
	mkdir ./dist

clean:
	rm -rf ./dist

.PHONY: mod
mod:
	go mod download

deps: mod
	go mod tidy
	git diff --exit-code

.PHONY: build
build: bin mod
	GOOS=linux CGO_ENABLED=0 go build -v -o dist/bootstrap .

.PHONY: build-deployment
build-deployment: dist mod
	GOARCH=arm64 GOOS=linux CGO_ENABLED=0 go build -v -o dist/bootstrap .