const fs = require('fs');
const path = require('path');
const postcss = require('postcss');

async function build() {
  const inputPath = path.resolve(__dirname, '../wwwroot/css/app.css');
  const outputPath = path.resolve(__dirname, '../wwwroot/css/site.css');

  if (!fs.existsSync(inputPath)) {
    console.error('Input CSS file not found:', inputPath);
    process.exit(1);
  }

  const tailwind = require('tailwindcss');
  const autoprefixer = require('autoprefixer');

  const css = fs.readFileSync(inputPath, 'utf8');

  try {
    const result = await postcss([tailwind, autoprefixer]).process(css, { from: inputPath, to: outputPath });
    fs.writeFileSync(outputPath, result.css, 'utf8');
    if (result.map) {
      fs.writeFileSync(outputPath + '.map', result.map.toString(), 'utf8');
    }
    console.log('Wrote', outputPath);
  } catch (err) {
    console.error('Error processing CSS:', err);
    process.exit(1);
  }
}

build();
