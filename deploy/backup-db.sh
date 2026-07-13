#!/usr/bin/env bash
# Backup diário do Postgres. Agendar no cron (ver DEPLOY.md):
#   0 5 * * * /opt/rpgworkspace/rpgworkspace-api/deploy/backup-db.sh >> /var/log/rpgworkspace-backup.log 2>&1
set -euo pipefail

BASE=/opt/rpgworkspace
BACKUP_DIR="$BASE/backups"
STAMP=$(date +%F_%H%M)
FILE="$BACKUP_DIR/rpgworkspace_$STAMP.dump"

mkdir -p "$BACKUP_DIR"

cd "$BASE/rpgworkspace-api/deploy"
docker compose exec -T db pg_dump -U rpgworkspace -d rpgworkspace -Fc > "$FILE"
echo "backup ok: $FILE ($(du -h "$FILE" | cut -f1))"

# Mantém os 14 mais recentes no disco local.
ls -1t "$BACKUP_DIR"/rpgworkspace_*.dump | tail -n +15 | xargs -r rm

# Cópia para fora da máquina (obrigatório — disco do VPS não é backup).
# Configurar uma vez com `rclone config`, remote chamado "offsite" (Backblaze B2 ou
# Cloudflare R2, tier grátis). Sem o remote configurado, o passo é pulado com aviso.
if command -v rclone >/dev/null && rclone listremotes 2>/dev/null | grep -q '^offsite:'; then
  rclone copy "$FILE" offsite:aventurario-backups/
  echo "offsite ok"
else
  echo "AVISO: remote rclone 'offsite' não configurado — backup só existe no disco do VPS"
fi
