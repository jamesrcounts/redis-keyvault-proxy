SHELL := /bin/bash
REGISTRY=acrrediskeyvaultproxy
REPOSITORY=cacheworker
# TAGS:=$$(pwd)
TAGS=$$(az acr repository show-tags \
			--name ${REGISTRY} \
			--repository ${REPOSITORY} \
			--orderby time_desc \
			--output tsv)

default: apply

info:
	T=($(TAGS)); echo -e "image = \"$(REPOSITORY):$${T[0]}\"" > variables.auto.tfvars

clean: connect
	terraform destroy -auto-approve

connect:
	terraform init -get-plugins=false -get=false

init:
	terraform init -upgrade

format:
	terraform fmt -recursive

show: 
	T=$$(mktemp); \
		terraform show -no-color > $${T}; \
		code $${T}

validate:
	terraform validate

apply: info connect format validate
	terraform apply 