from math import sin, cos, pi

offset = 1.1
scale = 128

total_values = 0
sine_values = ""
cosine_values = ""

for deg in range(0, 360, 5):
    rad = deg * pi / 180
    
    sine = sin(rad)
    cosine = cos(rad)
    
    offset_sine = sine + offset
    offset_cosine = cosine + offset
    
    offset_scaled_sine = int(offset_sine * scale)
    offset_scaled_cosine = int(offset_cosine * scale)
    
    #- Sines' array
    if sine_values != "":
        sine_values += f", {offset_scaled_sine}"
    else:
        sine_values = f"{offset_scaled_sine}"

    #- Cosines' array
    if cosine_values != "":
        cosine_values += f", {offset_scaled_cosine}"
    else:
        cosine_values = f"{offset_scaled_cosine}"
        
    total_values += 1

    print(f"{deg}, {rad} => {offset_scaled_sine}, {offset_scaled_cosine}")
    

#- Outputting array format

arr_start = "{"
arr_end = "}"

print()
print(f"#define TOTAL_VALUES {total_values}")
print()
print(f"uint16_t const sines[TOTAL_VALUES] = {arr_start} {sine_values} {arr_end};")
print()
print(f"uint16_t const cosines[TOTAL_VALUES] = {arr_start} {cosine_values} {arr_end};")
print()

#- END
print()