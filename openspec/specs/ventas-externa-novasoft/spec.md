# ventas-externa-novasoft Specification

## Purpose
TBD - created by archiving change create-venta-externa-novasoft. Update Purpose after archive.
## Requirements
### Requirement: Registrar venta externa de Novasoft
El sistema SHALL exponer un endpoint propio que reciba el JSON de una venta
originada en el sistema externo Novasoft, la persista mediante
`sp_Create_ventaExternaNovasoft`, y la envíe a la DIAN siguiendo el mismo
flujo de facturación electrónica ya usado para las ventas del POS.

#### Scenario: Creación exitosa de una venta externa
- **WHEN** se envía `POST /ventas-externa-novasoft` con el JSON de la venta en
  el cuerpo de la solicitud
- **THEN** el sistema ejecuta `sp_Create_ventaExternaNovasoft` pasando el JSON
  recibido como el parámetro `@ventaExternaNovasoft`, obteniendo el `idVenta`
  generado

#### Scenario: Error al ejecutar el stored procedure
- **WHEN** la ejecución de `sp_Create_ventaExternaNovasoft` lanza una excepción
  (por ejemplo, de conexión o de datos)
- **THEN** el sistema registra el error en el log y responde `400 Bad Request`
  con un mensaje descriptivo, sin exponer detalles sensibles de la conexión

#### Scenario: Envío exitoso a la DIAN tras crear la venta
- **WHEN** la venta externa se creó correctamente y el proveedor externo de
  facturación electrónica acepta el documento
- **THEN** el sistema registra la respuesta DIAN, genera el PDF de la
  factura, intenta enviarlo por correo al cliente, y responde `200 OK` con
  `idVenta` y el número de documento DIAN asignado

#### Scenario: La venta se crea pero la DIAN rechaza o no responde
- **WHEN** la venta externa se creó correctamente pero la llamada al
  proveedor externo de facturación electrónica falla o es rechazada
- **THEN** el sistema responde `200 OK` (no `400`, porque la venta sí se
  creó) con `idVenta` y el detalle del error de la API externa

#### Scenario: La venta y el envío a la DIAN son exitosos pero el correo falla
- **WHEN** el envío del correo con la factura al cliente falla después de un
  envío exitoso a la DIAN
- **THEN** el sistema responde `200 OK` indicando que la venta se creó pero
  el envío del correo falló, sin que ese fallo afecte el resultado de la
  creación de la venta ni del envío a la DIAN

