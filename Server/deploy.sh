#!/bin/bash
podman build -t localhost/logistic .
podman push localhost/logistic
podman rmi localhost/logistic