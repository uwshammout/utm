#include "ModbusRTUSlave.h"
#include "CyclicTaskExecutor.h"
#include "SimData.h"

using namespace cblk;

#define SLAVE_ID                  0x0001
#define BAUD_RATE                 115200
#define FILL_INTERVAL_MS          10

ModbusRTUSlave modbus(Serial);
CyclicTaskExecutor execFill(fill_sim_data, FILL_INTERVAL_MS);

void setup() {
  init_sim_data();

  modbus.configureHoldingRegisters(holding_registers, TOTAL_HOLDING_REGISTERS);
  modbus.begin(SLAVE_ID, BAUD_RATE);
}


void loop() {
  unsigned long currentTime_ms = millis();
  
  execFill.update(currentTime_ms);
  modbus.poll();
}
