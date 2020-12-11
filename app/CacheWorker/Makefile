default: push

push:
	az acr build \
		--image cacheworker:latest \
		--image cacheworker:{{.Run.ID}} \
		--registry acrrediskeyvaultproxy \
		--file Dockerfile .