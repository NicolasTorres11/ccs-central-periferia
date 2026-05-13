# Pruebas y cobertura

## Comando

```bash
dotnet test CCS.slnx --disable-build-servers -m:1 --collect:"XPlat Code Coverage" --settings coverlet.runsettings
```

## Resultado validado

Fecha de validacion local: 2026-05-13

La solucion ejecuta pruebas unitarias y de endpoints HTTP sobre:

- Dominio
- Aplicacion
- Infraestructura
- APIs locales
- Components tipo Functions simuladas

El objetivo del entregable es mantener cobertura de lineas superior al 50%. En la ultima validacion local se ejecutaron 45 pruebas y la cobertura global por union de lineas fue:

```text
TOTAL: 92.03% (381/414)
```

Los reportes Cobertura se generan localmente bajo `tests/**/TestResults/**/coverage.cobertura.xml`, ruta ignorada por git para evitar cargar resultados temporales.

Para calcular el porcentaje global desde los XML generados se toma la union de archivo/linea cubierta, evitando duplicar ensamblados referenciados por varios proyectos de prueba:

```bash
ruby -rrexml/document -e 'files=Dir["tests/**/TestResults/**/coverage.cobertura.xml"]; latest_by_proj={}; files.each{|f| proj=f.split("/")[1]; latest_by_proj[proj]=f if !latest_by_proj[proj] || File.mtime(f)>File.mtime(latest_by_proj[proj])}; lines={}; latest_by_proj.each_value do |f| doc=REXML::Document.new(File.read(f)); doc.elements.each("//class") do |cls| file=cls.attributes["filename"]; cls.elements.each(".//line") do |ln| key=[file,ln.attributes["number"]]; lines[key] ||= false; lines[key] ||= ln.attributes["hits"].to_i > 0; end; end; end; covered=lines.values.count(true); valid=lines.length; puts "TOTAL: %.2f%% (%d/%d)" % [covered*100.0/valid, covered, valid]'
```
