# OpenBoxes Mobile Blazor (IIS)

Cette application est une conversion initiale de la version mobile React Native vers Blazor Server.

## Fonctionnalités portées

- Authentification OpenBoxes (`/login`, `/logout`, `/getAppContext`)
- Session utilisateur persistée par cookie serveur
- Sélection de location (`/locations`, `/chooseLocation/{id}`)
- Produits (`/generic/product`, `/mobile/products/search`)
- Picking de base (`/facilities/{facilityId}/pick-tasks`)
- Dashboard avec entrées migrées et placeholders pour les flux restants

## Configuration

Modifier la base API dans `appsettings.json`:

```json
"OpenBoxesApi": {
  "BaseUrl": "https://votre-serveur/openboxes/api"
}
```

Optionnel (si hébergement sous sous-chemin IIS et besoin de forcer le path base):

```json
"PathBase": "/openboxesmobile"
```

## Exécution locale

```bash
dotnet restore
dotnet run
```

## Publication pour IIS

```bash
dotnet publish -c Release -o ../publish
```

Déployer le contenu de `../publish/` sur IIS.

Pré-requis serveur IIS:

- Installer le .NET Hosting Bundle correspondant au runtime
- Créer un site/app IIS pointant sur le dossier publié (racine du publish)
- Pool d'application en `No Managed Code`
- Autoriser l'accès HTTPS vers l'API OpenBoxes backend

Important:

- Ne pas pointer IIS vers `wwwroot` directement.
- Le path IIS doit viser le dossier qui contient `web.config` et `OpenBoxesMobile.Blazor.dll`.
- Pour un sous-chemin (ex: `/openboxesmobile`), créer une **Application IIS** (pas un simple dossier virtuel statique) vers le dossier publish.
- Le `base href` est désormais calculé dynamiquement depuis l'URL courante pour conserver CSS/JS.
