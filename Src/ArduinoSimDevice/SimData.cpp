#include "SimData.h"

#define DATA_ANALOG_READ_ESP32      0x00
#define DATA_ANALOG_READ_SIMPLE     0x01
#define DATA_GENERIC                0x02

#define SIM_DATA_TYPE  DATA_ANALOG_READ_ESP32

uint16_t holding_registers[TOTAL_HOLDING_REGISTERS];

//- Mapping
#define FCTotalVoltage       holding_registers[0]
#define FCTotalCurrentVh     holding_registers[1]
#define FCTotalCurrentVl     holding_registers[2]
#define FCCellVl             holding_registers[3]
#define FCCellV2             holding_registers[4]
#define FCCellV3             holding_registers[5]
#define FCCellV4             holding_registers[6]
#define FCCellV5             holding_registers[7]
#define FCCellV6             holding_registers[8]
#define FCCellV7             holding_registers[9]
#define FCCellV8             holding_registers[10]
#define FCCellV9             holding_registers[11]
#define ELTotalVoltage       holding_registers[12]
#define ELTotalCurrentVh     holding_registers[13]
#define ELTotalCurrentVl     holding_registers[14]


#if SIM_DATA_TYPE == DATA_ANALOG_READ_ESP32

#include <algorithm>

#define ADC_MAX_VOLTAGE               3.3
#define ADC_QNT_NUMBERS               4096
#define ADC_VAL_TO_VOLT               ADC_MAX_VOLTAGE / ADC_QNT_NUMBERS
#define VOLTAGE_SCALING               1000
#define ACS712_SENSITIVITY            0.1
#define SAMPLES_PER_INPUT             50
#define SAMPLING_DELAY_MS             0

static uint16_t samples_buff[SAMPLES_PER_INPUT];

#define SET_REGISTER_ZERO(__reg) { holding_registers[__reg] = 0; }
#define SET_REGISTER(__reg,__pin) {                                        \
                                                                           \
        pinMode(__pin, INPUT);                                             \
                                                                           \
        double __value = 0.0;                                              \
                                                                           \
        for (int __count = 0; __count < SAMPLES_PER_INPUT; __count++) {    \
                                                                           \
            samples_buff[__count] = analogRead(__pin);                     \
                                                                           \
            if (__count != SAMPLES_PER_INPUT - 1 &&                        \
                SAMPLING_DELAY_MS > 0) {                                   \
                                                                           \
                delay(SAMPLING_DELAY_MS);                                  \
            }                                                              \
        }                                                                  \
                                                                           \
        std::sort(samples_buff, samples_buff + SAMPLES_PER_INPUT);         \
                                                                           \
        __value = samples_buff[SAMPLES_PER_INPUT / 2] * ADC_VAL_TO_VOLT;   \
                                                                           \
        holding_registers[__reg] = (uint16_t)(__value * VOLTAGE_SCALING);  \
                                                                           \
        pinMode(__pin, INPUT_PULLDOWN);                                    \
    }
#define SET_REGISTER_ACS712(__reg,__pin,__zero_point) {                    \
                                                                           \
        double __value = 0.0;                                              \
                                                                           \
        for (int __count = 0; __count < SAMPLES_PER_INPUT; __count++) {    \
                                                                           \
            samples_buff[__count] = analogRead(__pin);                     \
                                                                           \
            if (__count != SAMPLES_PER_INPUT - 1 &&                        \
                SAMPLING_DELAY_MS > 0) {                                   \
                                                                           \
                delay(SAMPLING_DELAY_MS);                                  \
            }                                                              \
        }                                                                  \
                                                                           \
        std::sort(samples_buff, samples_buff + SAMPLES_PER_INPUT);         \
                                                                           \
        __value = samples_buff[SAMPLES_PER_INPUT / 2] * ADC_VAL_TO_VOLT -  \
                           __zero_point;                                   \
                                                                           \
        if (__value < 0) __value *= -1;                                    \
                                                                           \
        holding_registers[__reg] = (uint16_t)(                             \
                (__value / ACS712_SENSITIVITY) * VOLTAGE_SCALING           \
            );                                                             \
    }

enum InputState : int {
  INPUT_STATE_A01 = 0,
  INPUT_STATE_A02, INPUT_STATE_A03, INPUT_STATE_A04, INPUT_STATE_A05,
  INPUT_STATE_A06, INPUT_STATE_A07, INPUT_STATE_A08, INPUT_STATE_A09,
  INPUT_STATE_A10, INPUT_STATE_A11, INPUT_STATE_A12, INPUT_STATE_A13,
  INPUT_STATE_A14, INPUT_STATE_A15,
  INPUT_STATE_MAX
};

void init_sim_data() {}

void fill_sim_data() {
  static InputState input_state = INPUT_STATE_A01;

  switch (input_state) {
    case INPUT_STATE_A01: SET_REGISTER(0, 36);     break;
    case INPUT_STATE_A02: SET_REGISTER_ACS712(1, 39, 2.3);    break;
    case INPUT_STATE_A03: SET_REGISTER_ZERO(2);    break;
    case INPUT_STATE_A04: SET_REGISTER(3, 32);     break;
    case INPUT_STATE_A05: SET_REGISTER(4, 33);     break;
    case INPUT_STATE_A06: SET_REGISTER(5, 25);     break;
    case INPUT_STATE_A07: SET_REGISTER(6, 26);     break;
    case INPUT_STATE_A08: SET_REGISTER(7, 27);     break;
    case INPUT_STATE_A09: SET_REGISTER(8, 14);     break;
    case INPUT_STATE_A10: SET_REGISTER(9, 12);     break;
    case INPUT_STATE_A11: SET_REGISTER(10, 13);    break;
    case INPUT_STATE_A12: SET_REGISTER(11, 15);    break;
    case INPUT_STATE_A13: SET_REGISTER(12, 2);     break;
    case INPUT_STATE_A14: SET_REGISTER_ACS712(13, 4, 2.3);    break;
    case INPUT_STATE_A15: SET_REGISTER_ZERO(14);   break;
  }


  input_state = (InputState)(((int)input_state) + 1);

  if (input_state >= INPUT_STATE_MAX) {
    input_state = INPUT_STATE_A01;
  }
}






#elif SIM_DATA_TYPE == DATA_ANALOG_READ_SIMPLE

static const byte pot_pins[6] = { A0, A1, A2, A3, A4, A5 };

void init_sim_data() {
  pinMode(A0, INPUT);
  pinMode(A1, INPUT);
  pinMode(A2, INPUT);
  pinMode(A3, INPUT);
  pinMode(A4, INPUT);
  pinMode(A5, INPUT);
}

void fill_sim_data() {
  holding_registers[0] = map(analogRead(pot_pins[0]), 0, 1023, 0, 255);
  holding_registers[1] = map(analogRead(pot_pins[1]), 0, 1023, 0, 255);
  holding_registers[2] = map(analogRead(pot_pins[2]), 0, 1023, 0, 255);
  holding_registers[3] = map(analogRead(pot_pins[3]), 0, 1023, 0, 255);
  holding_registers[4] = map(analogRead(pot_pins[4]), 0, 1023, 0, 255);
  holding_registers[5] = map(analogRead(pot_pins[5]), 0, 1023, 0, 255);
  holding_registers[6] = 100;
  holding_registers[7] = 100;
  holding_registers[8] = 1000;
  holding_registers[9] = 1000;
  holding_registers[10] = 0;
  holding_registers[11] = 1;
  holding_registers[12] = 0;
  holding_registers[13] = 10;
  holding_registers[14] = 0;
  holding_registers[15] = 100;
}

#elif  SIM_DATA_TYPE == DATA_GENERIC

#define TOTAL_VALUES 72

uint16_t const sines[TOTAL_VALUES] = { 140, 151, 163, 173, 184, 194, 204, 214, 223, 231, 238, 245, 251, 256, 261, 264, 266, 268, 268, 268, 266, 264, 261, 256, 251, 245, 238, 231, 223, 214, 204, 194, 184, 173, 163, 151, 140, 129, 118, 107, 97, 86, 76, 67, 58, 50, 42, 35, 29, 24, 20, 17, 14, 13, 12, 13, 14, 17, 20, 24, 29, 35, 42, 50, 58, 67, 76, 86, 97, 107, 118, 129 };
uint16_t const cosines[TOTAL_VALUES] = { 268, 268, 266, 264, 261, 256, 251, 245, 238, 231, 223, 214, 204, 194, 184, 173, 163, 151, 140, 129, 118, 107, 97, 86, 76, 67, 58, 50, 42, 35, 29, 24, 20, 17, 14, 13, 12, 13, 14, 17, 20, 24, 29, 35, 42, 50, 58, 67, 76, 86, 97, 107, 118, 129, 140, 151, 163, 173, 184, 194, 204, 214, 223, 231, 238, 245, 251, 256, 261, 264, 266, 268 };

void init_sim_data() {}

static uint16_t data_index = 0;
static uint16_t data_index2 = 5;

void fill_sim_data() {
  
  FCTotalVoltage = sines[data_index];
  FCTotalCurrentVh = cosines[data_index];
  FCTotalCurrentVl = cosines[data_index2];

  uint16_t tenth_part = FCTotalVoltage / 10;
  FCCellVl = FCTotalVoltage - tenth_part * 9;
  FCCellV2 = FCTotalVoltage - tenth_part * 8;
  FCCellV3 = FCTotalVoltage - tenth_part * 7;
  FCCellV4 = FCTotalVoltage - tenth_part * 6;
  FCCellV5 = FCTotalVoltage - tenth_part * 5;
  FCCellV6 = FCTotalVoltage - tenth_part * 4;
  FCCellV7 = FCTotalVoltage - tenth_part * 3;
  FCCellV8 = FCTotalVoltage - tenth_part * 2;
  FCCellV9 = FCTotalVoltage - tenth_part;
  ELTotalVoltage = sines[data_index];
  ELTotalCurrentVh = cosines[data_index];
  ELTotalCurrentVl = cosines[data_index2];

  data_index++;
  data_index2++;

  if (data_index >= TOTAL_VALUES) {
    data_index = 0;
  }
  if (data_index2 >= TOTAL_VALUES) {
    data_index2 = 0;
  }
}

#else
#  error "SIM_DATA_TYPE not defined"
#endif
