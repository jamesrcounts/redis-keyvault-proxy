default: push

build:
	az acr build \
		--no-push \
		--image cacheworker:latest \
		--image cacheworker:{{.Run.ID}} \
		--registry acrrediskeyvaultproxy \
		--file Dockerfile .

push:
	az acr build \
		--image cacheworker:latest \
		--image cacheworker:{{.Run.ID}} \
		--registry acrrediskeyvaultproxy \
		--file Dockerfile .