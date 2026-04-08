#!/usr/bin/env bash

set -euo pipefail

PROJECT_ID="${PROJECT_ID:-issuesense-492717}"
REGION="${REGION:-us-central1}"
SERVICE_NAME="${SERVICE_NAME:-issuesense}"
REPOSITORY_NAME="${REPOSITORY_NAME:-issuesense}"
IMAGE_NAME="${IMAGE_NAME:-issuesense}"
DATABASE_NAME="${DATABASE_NAME:-IssueSenseDb}"
COMPLAINTS_COLLECTION="${COMPLAINTS_COLLECTION:-complaints}"
USERS_COLLECTION="${USERS_COLLECTION:-users}"
OPENAI_MODEL="${OPENAI_MODEL:-gpt-5.4-nano}"
OPENAI_ENDPOINT="${OPENAI_ENDPOINT:-https://api.openai.com/v1/responses}"
OPENAI_USE_MOCK_FALLBACK="${OPENAI_USE_MOCK_FALLBACK:-true}"
SEED_DATA="${SEED_DATA:-false}"
ALLOW_UNAUTHENTICATED="${ALLOW_UNAUTHENTICATED:-true}"
ENABLE_OPENAI="${ENABLE_OPENAI:-true}"
SECRET_NAME="${SECRET_NAME:-OPENAI_API_KEY}"


if ! command -v gcloud >/dev/null 2>&1; then
  echo "Error: gcloud CLI is not installed." >&2
  exit 1
fi

if [[ -z "${MONGODB_CONNECTION_STRING:-}" ]]; then
  read -r -s -p "Enter MongoDB connection string: " MONGODB_CONNECTION_STRING
  echo
fi

if [[ -z "${MONGODB_CONNECTION_STRING:-}" ]]; then
  echo "Error: MONGODB_CONNECTION_STRING is required." >&2
  exit 1
fi

echo "Using project: ${PROJECT_ID}"
echo "Using region: ${REGION}"
echo "Using service: ${SERVICE_NAME}"

gcloud config set project "${PROJECT_ID}" >/dev/null

echo "Enabling required Google Cloud services..."
gcloud services enable \
  run.googleapis.com \
  cloudbuild.googleapis.com \
  artifactregistry.googleapis.com \
  secretmanager.googleapis.com >/dev/null

echo "Ensuring Artifact Registry repository exists..."
if ! gcloud artifacts repositories describe "${REPOSITORY_NAME}" \
  --location="${REGION}" >/dev/null 2>&1; then
  gcloud artifacts repositories create "${REPOSITORY_NAME}" \
    --repository-format=docker \
    --location="${REGION}" \
    --description="IssueSense containers"
fi

if [[ "${ENABLE_OPENAI}" == "true" ]]; then
  echo "Ensuring Secret Manager secret exists..."
  if ! gcloud secrets describe "${SECRET_NAME}" >/dev/null 2>&1; then
    if [[ -z "${OPENAI_API_KEY:-}" ]]; then
      read -r -s -p "Enter OpenAI API key: " OPENAI_API_KEY
      echo
    fi

    if [[ -z "${OPENAI_API_KEY:-}" ]]; then
      echo "Error: OPENAI_API_KEY is required because secret ${SECRET_NAME} does not exist yet." >&2
      exit 1
    fi

    printf '%s' "${OPENAI_API_KEY}" | gcloud secrets create "${SECRET_NAME}" --data-file=-
  else
    if [[ -n "${OPENAI_API_KEY:-}" ]]; then
      printf '%s' "${OPENAI_API_KEY}" | gcloud secrets versions add "${SECRET_NAME}" --data-file=-
    else
      echo "OpenAI secret ${SECRET_NAME} already exists. Reusing existing secret version."
    fi
  fi
fi

IMAGE_URI="${REGION}-docker.pkg.dev/${PROJECT_ID}/${REPOSITORY_NAME}/${IMAGE_NAME}"

echo "Building container image: ${IMAGE_URI}"
gcloud builds submit --tag "${IMAGE_URI}"

DEPLOY_ARGS=(
  run deploy "${SERVICE_NAME}"
  --image "${IMAGE_URI}"
  --region "${REGION}"
  --platform managed
  --set-env-vars "ASPNETCORE_ENVIRONMENT=Production,SeedData=${SEED_DATA},OpenAI__Enabled=${ENABLE_OPENAI},OpenAI__Model=${OPENAI_MODEL},OpenAI__Endpoint=${OPENAI_ENDPOINT},OpenAI__UseMockFallback=${OPENAI_USE_MOCK_FALLBACK},MongoDb__ConnectionString=${MONGODB_CONNECTION_STRING},MongoDb__DatabaseName=${DATABASE_NAME},MongoDb__ComplaintsCollectionName=${COMPLAINTS_COLLECTION},MongoDb__UsersCollectionName=${USERS_COLLECTION}"
)

if [[ "${ALLOW_UNAUTHENTICATED}" == "true" ]]; then
  DEPLOY_ARGS+=(--allow-unauthenticated)
else
  DEPLOY_ARGS+=(--no-allow-unauthenticated)
fi

if [[ "${ENABLE_OPENAI}" == "true" ]]; then
  PROJECT_NUMBER="$(gcloud projects describe "${PROJECT_ID}" --format='value(projectNumber)')"
  gcloud secrets add-iam-policy-binding "${SECRET_NAME}" \
    --member="serviceAccount:${PROJECT_NUMBER}-compute@developer.gserviceaccount.com" \
    --role="roles/secretmanager.secretAccessor" >/dev/null

  DEPLOY_ARGS+=(--update-secrets "OpenAI__ApiKey=${SECRET_NAME}:latest")
fi

echo "Deploying to Cloud Run..."
gcloud "${DEPLOY_ARGS[@]}"

SERVICE_URL="$(gcloud run services describe "${SERVICE_NAME}" --region "${REGION}" --format='value(status.url)')"

echo
echo "Deployment complete."
echo "Service URL: ${SERVICE_URL}"
echo "Health check: ${SERVICE_URL}/health"
