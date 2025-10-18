const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

// Determine backend root and API configuration folder
const backendRoot = path.resolve(__dirname, '..');
const apiConfigDir = path.join(backendRoot, '02.Service', 'API');

// Support environment specific overrides
const env = process.env.ASPNETCORE_ENVIRONMENT;
const baseConfigPath = path.join(apiConfigDir, 'appsettings.json');
let config = {};

if (fs.existsSync(baseConfigPath)) {
  config = JSON.parse(fs.readFileSync(baseConfigPath, 'utf8'));
}

if (env) {
  const envConfigPath = path.join(apiConfigDir, `appsettings.${env}.json`);
  if (fs.existsSync(envConfigPath)) {
    const envConfig = JSON.parse(fs.readFileSync(envConfigPath, 'utf8'));
    config = { ...config, ...envConfig };
  }
}

const dx = config.DevExpressNuget;
if (!dx) {
  console.error('DevExpressNuget section not found in appsettings.');
  process.exit(1);
}

const source = dx.Source && dx.Source.trim();
if (!source) {
  console.error('DevExpress NuGet source is not configured.');
  process.exit(1);
}

const outputPath = path.join(backendRoot, 'NuGet.config');

if (!fs.existsSync(outputPath)) {
  fs.writeFileSync(outputPath, '<?xml version="1.0" encoding="utf-8"?><configuration></configuration>');
}

try {
  execSync(`dotnet nuget remove source DevExpress --configfile "${outputPath}"`, { stdio: 'ignore' });
} catch {}

const args = [
  'dotnet', 'nuget', 'add', 'source', source,
  '--name', 'DevExpress',
  '--configfile', outputPath
];

if (dx.User && dx.Password) {
  args.push('--username', dx.User);
  args.push('--password', dx.Password);
  args.push('--store-password-in-clear-text');
}

try {
  execSync(args.join(' '), { stdio: 'inherit' });
  console.log(`DevExpress NuGet source added to ${outputPath}`);
} catch (err) {
  console.error('Failed to configure DevExpress NuGet source.');
  process.exit(1);
}
