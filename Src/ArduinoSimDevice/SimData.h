#ifndef SimData_h
#define SimData_h


#include <Arduino.h>

#define TOTAL_HOLDING_REGISTERS   16

extern uint16_t holding_registers[];

void init_sim_data();
void fill_sim_data();


#endif
