from __future__ import annotations

from dataclasses import asdict, dataclass, field
from pathlib import Path
from typing import Dict


def _repo_root() -> Path:
    return Path(__file__).resolve().parents[2]


def _normalize_api_health_url(value: str) -> str:
    if not value:
        return value

    normalized = value.strip()
    if normalized.endswith(":3030/health"):
        return normalized[:-len("/health")] + "/api/health"

    return normalized


@dataclass
class GeneralConfig:
    repository_url: str = "https://github.com/desenvolvimentoliotecnica/Portal-Lioconnecta.git"
    local_repo_path: str = str(_repo_root() / "artifacts" / "deploy" / "source-cache")
    api_project_path: str = str(_repo_root() / "src" / "PortalLioConnecta.Api" / "PortalLioConnecta.Api.csproj")
    api_test_project_path: str = str(_repo_root() / "tests" / "PortalLioConnecta.Api.Tests" / "PortalLioConnecta.Api.Tests.csproj")
    frontend_root_path: str = str(_repo_root() / "LioConnecta")
    package_script_path: str = str(_repo_root() / "scripts" / "package-lioconnecta.ps1")
    artifact_root: str = str(_repo_root() / "artifacts" / "deploy" / "gui")
    run_api_tests: bool = True
    run_frontend_tests: bool = True


@dataclass
class EnvironmentConfig:
    name: str = ""
    branch: str = ""
    host: str = ""
    port: int = 22
    username: str = ""
    auth_mode: str = "password"  # password | key
    password: str = ""
    ssh_key_path: str = ""
    host_key_fingerprint: str = ""
    strict_host_key_checking: bool = False
    deploy_path: str = ""
    frontend_target_path: str = ""
    api_service: str = ""
    frontend_health_url: str = ""
    api_health_url: str = ""
    post_deploy_command: str = ""


@dataclass
class AppConfig:
    general: GeneralConfig = field(default_factory=GeneralConfig)
    environments: Dict[str, EnvironmentConfig] = field(
        default_factory=lambda: {
            "DEV": EnvironmentConfig(
                name="DEV",
                branch="Lioconnecta_DEV",
                host_key_fingerprint="SHA256:oDGR7PLQZE7ShzUKZY1NynfRt8dY+XCf1PLL3E1pakM",
            ),
            "HML": EnvironmentConfig(name="HML", branch="Lioconnecta_HML"),
            "PRD": EnvironmentConfig(name="PRD", branch="Lioconnecta_PRD"),
        }
    )

    def to_dict(self) -> dict:
        return {
            "general": asdict(self.general),
            "environments": {key: asdict(value) for key, value in self.environments.items()},
        }

    @classmethod
    def from_dict(cls, raw: dict | None) -> "AppConfig":
        config = cls()
        if not raw:
            return config

        general_raw = raw.get("general") or {}
        config.general = GeneralConfig(**{**asdict(config.general), **general_raw})

        envs_raw = raw.get("environments") or {}
        merged: Dict[str, EnvironmentConfig] = {}
        for name, default_env in config.environments.items():
            env_raw = envs_raw.get(name) or {}
            merged[name] = EnvironmentConfig(**{**asdict(default_env), **env_raw})
            merged[name].api_health_url = _normalize_api_health_url(
                merged[name].api_health_url
            )

        for name, env_raw in envs_raw.items():
            if name not in merged:
                merged[name] = EnvironmentConfig(**env_raw)
                merged[name].api_health_url = _normalize_api_health_url(
                    merged[name].api_health_url
                )

        config.environments = merged
        return config
