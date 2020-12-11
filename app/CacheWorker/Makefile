REGISTRY=acrrediskeyvaultproxy
REPOSITORY=cacheworker

default: push

info:
	az acr repository show-tags \
		--name ${REGISTRY} \
		--repository ${REPOSITORY} \
		--orderby time_desc \
		--output table

push:
	az acr build \
		--image ${REPOSITORY}:latest \
		--image ${REPOSITORY}:{{.Run.ID}} \
		--registry ${REGISTRY} \
		--file Dockerfile .