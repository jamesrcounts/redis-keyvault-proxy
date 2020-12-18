default: base-layer

app-layer:
	cd infrastructure/app && make

base-layer:
	cd infrastructure/azure && make

