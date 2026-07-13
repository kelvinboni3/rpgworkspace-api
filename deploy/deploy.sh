#!/usr/bin/env bash
# Deploy/atualização do stack no VPS. Rodar como o usuário dono de /opt/rpgworkspace.
set -euo pipefail

BASE=/opt/rpgworkspace

echo "==> Atualizando repositórios"
git -C "$BASE/rpgworkspace-api" pull --ff-only
git -C "$BASE/rpgworkspace-frontend" pull --ff-only

echo "==> Build do front (container Node descartável — nada de Node no host)"
docker run --rm \
  -v "$BASE/rpgworkspace-frontend":/app -w /app \
  -e VITE_API_BASE_URL=/api \
  node:20-alpine sh -c "npm ci && npm run build"

echo "==> Subindo o stack"
cd "$BASE/rpgworkspace-api/deploy"
docker compose up -d --build

echo "==> Pronto. Health:"
sleep 5
curl -fsS http://localhost/health/ready || echo "(health check falhou — ver: docker compose logs api)"
